using UnityEngine;

namespace MarTools
{
    [ExecuteAlways]
    public class DollyCartLineBehavior : MonoBehaviour
    {
        public Vector3 offset = Vector3.zero;
        public float damping = 10f;

        [SerializeField] LineBehavior lineBehavior;
        [SerializeField] Transform trackedTarget;

        private Vector3 velocity;

        private void Update()
        {
            if (!lineBehavior || !trackedTarget) return;

            var worldPoints = lineBehavior.smoothWorldPoints;
            if (worldPoints == null || worldPoints.Count < 2) return;

            float minSqrDistance = float.MaxValue;
            Vector3 bestPoint = worldPoints[0];

            // Precompute the projected target position (on transform.right).
            Vector3 targetProjected = Vector3.Project(trackedTarget.position, transform.right);

            // Iterate over each segment [i..i+1]
            for (int i = 0; i < worldPoints.Count - 1; i++)
            {
                Vector3 start = worldPoints[i];
                Vector3 end = worldPoints[i + 1];

                // Project segment endpoints onto transform.right
                Vector3 startProjected = Vector3.Project(start, transform.right);
                Vector3 endProjected = Vector3.Project(end, transform.right);

                // We'll find how far 'targetProjected' is between startProjected & endProjected
                Vector3 segVec = endProjected - startProjected;
                float segLenSqr = segVec.sqrMagnitude;

                // If the segment is extremely small in this axis, skip
                if (segLenSqr < Mathf.Epsilon)
                    continue;

                // Param t along the segment in "projected space"
                // Dot(tarVec, segVec) / |segVec|^2
                Vector3 tarVec = targetProjected - startProjected;
                float t = Vector3.Dot(tarVec, segVec) / segLenSqr;
                t = Mathf.Clamp01(t);

                // Interpolate in 3D between the original points
                // at the same fraction t
                Vector3 candidate = Vector3.Lerp(start, end, t);

                // Now project this candidate in "transform.right" so we can
                // compare it to targetProjected the same way you did originally.
                Vector3 candidateProjected = Vector3.Project(candidate, transform.right);

                // Compare distances in the projected space (sqrMagnitude in the axis direction)
                Vector3 diff = candidateProjected - targetProjected;
                float distSqr = diff.sqrMagnitude;

                if (distSqr < minSqrDistance)
                {
                    minSqrDistance = distSqr;
                    bestPoint = candidate;
                }
            }

            // Add any user offset
            Vector3 targetPos = bestPoint + offset;

            if (Application.isPlaying)
            {
                // Smoothly move towards target
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPos,
                    ref velocity,
                    damping,
                    Mathf.Infinity,
                    Time.deltaTime
                );
            }
            else
            {
                // Set directly in edit mode
                transform.position = targetPos;
            }
        }

        private void OnDrawGizmos()
        {
            if (lineBehavior == null) return;

            var worldPoints = lineBehavior.smoothWorldPoints;
            if (worldPoints == null) return;

            foreach (var item in worldPoints)
            {
                Gizmos.DrawSphere(item, 0.1f);
            }
        }
    }
}
