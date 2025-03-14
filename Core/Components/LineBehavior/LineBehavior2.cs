using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarTools
{
    public class LineBehavior2 : MonoBehaviour
    {
        [System.Serializable]
        public class PointData
        {
            public Vector3 eulerRotation;
            public Vector3 position;

            public Quaternion rotation
            {
                get
                {
                    return Quaternion.Euler(eulerRotation);
                }
                set
                {
                    eulerRotation = value.eulerAngles;
                }
            }
        }

        public bool closedShape = false;
        public List<PointData> Points = new List<PointData>();

        [SerializeField]
        public int resolution = 5;
        [SerializeField]
        public float smoothingLength = 1;

        public List<PointData> GetSmoothPoints()
        {
            List<PointData> SmoothPoints = new List<PointData>();
            if (Points.Count == 0) return Points;

            var controlPoints = Points;

            if (resolution <= 0)
            {
                return Points;
            }

            if (Points.Count < 4) return Points; 

            (Vector3, Vector3)[] Tangents = new (Vector3, Vector3)[controlPoints.Count];
            for (int i = 0; i < controlPoints.Count; i++)
            {
                int next = (i + 1) % controlPoints.Count;
                int previous = (i - 1 + controlPoints.Count) % controlPoints.Count;


                Vector3 t1 = (controlPoints[next].position - controlPoints[i].position);
                Vector3 t2 = (controlPoints[previous].position - controlPoints[i].position);


                // HACK WITH THE ANGLE TO FLIP TANGENTS
                float angle = Vector3.SignedAngle(t1, t2, Vector3.up);
                Vector3 n = (Vector3.Cross(Vector3.Slerp(t1, t2, 0.5f), angle >= -180 && angle < 0 ? Vector3.up : Vector3.down).normalized * smoothingLength);

                if (!closedShape)
                {
                    if (i == 0 || i == controlPoints.Count - 1) n = Vector3.zero;
                }
                Tangents[i] = (controlPoints[i].position + n, controlPoints[i].position - n);
            }


            for (int i = 0; i < controlPoints.Count; i++)
            {
                int ind1 = i;
                int ind2 = (i + 1) % controlPoints.Count;

                var p1 = controlPoints[ind1];
                var p2 = controlPoints[ind2];

                var tan1 = Tangents[ind1];
                var tan2 = Tangents[ind2];

                if (!closedShape && i == controlPoints.Count - 1) continue;

                for (int j = 0; j < resolution; j++)
                {
                    float t = (float)j / (resolution - 1);

                    SmoothPoints.Add(new PointData()
                    {
                        position = BezierCurve(t, p1.position, tan1.Item2, tan2.Item1, p2.position),
                        eulerRotation = Quaternion.SlerpUnclamped(p1.rotation, p2.rotation, t).eulerAngles,
                    });
                }
            }

            return SmoothPoints;
        }

        public static Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            // p0, p1, p2, and p3 are control points for the Bezier curve
            Vector3 p = uuu * p0; // Start point influenced by p0
            p += 3 * uu * t * p1; // Control point, pulling towards p1
            p += 3 * u * tt * p2; // Control point, pulling towards p2
            p += ttt * p3; // End point influenced by p3

            return p;
        }

        public float totalDistance
        {
            get
            {
                var Points = GetSmoothPoints();

                float distance = 0;
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    distance += Vector3.Distance(Points[i].position, Points[i + 1].position);
                }
                if (closedShape)
                {
                    distance += Vector3.Distance(Points[Points.Count - 1].position, Points[0].position);
                }
                return distance;
            }
        }

        public PointData GetClosestPoint(Vector3 pos, out float progress)
        {
            PointData closest = new PointData();

            float minDistance = float.MaxValue;
            float elapsedDistance = 0;
            progress = 0;

            var Points = GetSmoothPoints();

            for (int i = 0; i < Points.Count-1; i++)
            {
                PointData prev = Points[i];
                PointData next = Points[i+1];


                Vector3 closestPoint = Utilities.ClosestPointOnLineSegment(pos, prev.position, next.position, out float _);

                float sqrMagnitude = Vector3.SqrMagnitude(closestPoint - pos);
                if (minDistance >= sqrMagnitude)
                {
                    float distSqr = (closestPoint - prev.position).sqrMagnitude;
                    float t = distSqr / (prev.position - next.position).sqrMagnitude;
                    minDistance = sqrMagnitude;

                    closest.eulerRotation = Quaternion.SlerpUnclamped(prev.rotation, next.rotation, t).eulerAngles;
                    closest.position = closestPoint;

                    progress = (elapsedDistance+Mathf.Sqrt(distSqr)) / totalDistance;
                }

                elapsedDistance += Vector3.Distance(prev.position, next.position);
            }

            return closest;
        }

        public void InsertPointAt(float progress, Vector3 point)
        {
            progress = Mathf.Clamp01(progress);

            // If there are fewer than 2 control points, just add a new one.
            if (Points.Count < 2)
            {
                PointData newPoint = new PointData();
                newPoint.position = point;
                Points.Add(newPoint);
                return;
            }

            // Calculate the total length along the control points (using straight lines).
            float totalDistance = 0f;
            int count = Points.Count;
            if (closedShape)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector3 current = Points[i].position;
                    Vector3 next = Points[(i + 1) % count].position;
                    totalDistance += Vector3.Distance(current, next);
                }
            }
            else
            {
                for (int i = 0; i < count - 1; i++)
                {
                    totalDistance += Vector3.Distance(Points[i].position, Points[i + 1].position);
                }
            }

            // Determine the target distance along the polyline.
            float targetDistance = progress * totalDistance;

            // Find the segment where the target distance falls.
            float accumulatedDistance = 0f;
            int segmentIndex = -1;
            float segmentFraction = 0f;

            if (closedShape)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector3 start = Points[i].position;
                    Vector3 end = Points[(i + 1) % count].position;
                    float segLength = Vector3.Distance(start, end);
                    if (accumulatedDistance + segLength >= targetDistance)
                    {
                        segmentIndex = i;
                        segmentFraction = (targetDistance - accumulatedDistance) / segLength;
                        break;
                    }
                    accumulatedDistance += segLength;
                }
                // If progress is 1, targetDistance equals totalDistance.
                if (segmentIndex == -1)
                {
                    segmentIndex = count - 1;
                    segmentFraction = 1f;
                }
            }
            else
            {
                for (int i = 0; i < count - 1; i++)
                {
                    Vector3 start = Points[i].position;
                    Vector3 end = Points[i + 1].position;
                    float segLength = Vector3.Distance(start, end);
                    if (accumulatedDistance + segLength >= targetDistance)
                    {
                        segmentIndex = i;
                        segmentFraction = (targetDistance - accumulatedDistance) / segLength;
                        break;
                    }
                    accumulatedDistance += segLength;
                }
                if (segmentIndex == -1)
                {
                    segmentIndex = count - 2;
                    segmentFraction = 1f;
                }
            }

            // Interpolate between the two control points of the segment.
            PointData newPointData = new PointData();

            newPointData.position = point;


            //if (closedShape)
            //{
            //    PointData start = Points[segmentIndex];
            //    PointData end = Points[(segmentIndex + 1) % count];
            //    newPointData.position = Vector3.Lerp(start.position, end.position, segmentFraction);
            //    newPointData.rotation = Quaternion.Lerp(start.rotation, end.rotation, segmentFraction);
            //}
            //else
            //{
            //    PointData start = Points[segmentIndex];
            //    PointData end = Points[segmentIndex + 1];
            //    newPointData.position = Vector3.Lerp(start.position, end.position, segmentFraction);
            //    newPointData.rotation = Quaternion.Lerp(start.rotation, end.rotation, segmentFraction);
            //}

            // Insert the new control point into the control points list.
            // For non-closed shapes, insert after the starting point of the segment.
            // For closed shapes, if the segment is from the last to the first, append the new point.
            if (closedShape)
            {
                if (segmentIndex == count - 1)
                {
                    Points.Add(newPointData);
                }
                else
                {
                    Points.Insert(segmentIndex + 1, newPointData);
                }
            }
            else
            {
                Points.Insert(segmentIndex + 1, newPointData);
            }
        }


        public (int, int) GetClosestIndexes(float progress)
        {
            List<PointData> smoothPoints = GetSmoothPoints();
            if (smoothPoints.Count < 2)
            {
                return (0, 0);
            }

            progress = Mathf.Clamp01(progress);
            float targetDistance = progress * totalDistance;
            float accumulatedDistance = 0f;

            for (int i = 0; i < smoothPoints.Count - 1; i++)
            {
                float segmentDistance = Vector3.Distance(smoothPoints[i].position, smoothPoints[i + 1].position);
                if (accumulatedDistance + segmentDistance >= targetDistance)
                {
                    return (i, i + 1);
                }
                accumulatedDistance += segmentDistance;
            }

            // If progress is 1 or very close, handle the end of the curve.
            if (closedShape)
            {
                return (smoothPoints.Count - 1, 0);
            }
            else
            {
                return (smoothPoints.Count - 2, smoothPoints.Count - 1);
            }
        }


        public PointData GetPoint(float progress)
        {
            PointData point = new PointData();
            float elapsedDistance = 0;

            float requiredDistance = progress * totalDistance;

            var Points = GetSmoothPoints();

            for (int i = 0; i < Points.Count - 1; i++)
            {
                PointData prev = Points[i];
                PointData next = Points[i + 1];

                float segmentLength = Vector3.Distance(prev.position, next.position);
                elapsedDistance += segmentLength;
                if(elapsedDistance >= requiredDistance)
                {
                    float overshoot = elapsedDistance - requiredDistance;
                    float t = (segmentLength - overshoot) / segmentLength;

                    point.position = prev.position + (segmentLength - overshoot) * (next.position - prev.position).normalized;
                    point.eulerRotation = Quaternion.Slerp(prev.rotation, next.rotation, t).eulerAngles;

                    return point;
                }

            }

            return point;
        }


        private void OnDrawGizmos()
        {
            var Points = GetSmoothPoints();

            for (int i = 0; i < Points.Count - 1; i++)
            {
                Vector3 prev = Points[i].position;
                Vector3 next = Points[i + 1].position;

                Gizmos.DrawLine(prev, next);
            }

            foreach (var point in Points)
            {
                Gizmos.color = Color.white.SetAlpha(0.5f);
                Gizmos.DrawSphere(point.position, 0.05f);
                GizmosUtilities.DrawRotation(point.position, point.rotation, 0.5f);
            }
            foreach (var item in this.Points)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(item.position, 0.1f);
                GizmosUtilities.DrawRotation(item.position, item.rotation, 1f);
            }
        }

        public void SetRotationsToSlope()
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                Vector3 prev = Points[i].position;
                Vector3 next = Points[i + 1].position;
                Vector3 direction = (next - prev).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                Points[i].rotation = rotation;
            }

            if (closedShape)
            {
                Vector3 prev = Points[Points.Count - 1].position;
                Vector3 next = Points[0].position;
                Vector3 direction = (next - prev).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                Points[Points.Count - 1].rotation = rotation;
            }
            else
            {
                Points[Points.Count - 1].rotation = Points[Points.Count - 2].rotation;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LineBehavior2))]
    public class LineBehavior2Editor : Editor
    {
        public enum Mode
        {
            None,
            Position,
            Rotation,
        }

        Mode currentMode = Mode.None;
        LineBehavior2 script;

        private void OnEnable()
        {
            script = target as LineBehavior2;
            EditorApplication.update += UpdateEditor;
        }
        private void OnDisable()
        {
            EditorApplication.update -= UpdateEditor;
        }

        private void UpdateEditor()
        {
            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            Handles.color = Color.green;

            var smoothPoints = script.GetSmoothPoints();

            if(smoothPoints.Count >= 2)
            {
                for (int i = 0; i < smoothPoints.Count-1; i++)
                {
                    Handles.DrawLine(smoothPoints[i].position, smoothPoints[i + 1].position);
                }
            }


            if(Event.current.shift)
            {
                PointsControl();
            }
            else
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.W)
                    {
                        currentMode = Mode.Position;
                    }
                    else if (Event.current.keyCode == KeyCode.E)
                    {
                        currentMode = Mode.Rotation;
                    }
                }

                switch (currentMode)
                {
                    case Mode.Position:
                        PositioningTool();
                        break;
                    case Mode.Rotation:
                        RotationTool();
                        break;
                }
            }

        }

        private void PointsControl()
        {
            Vector3 cursorPosition = Vector3.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                cursorPosition = ray.GetPoint(distance);
            }

            Handles.color = Color.gray * 0.9f;
            Handles.DrawSolidDisc(cursorPosition, Vector3.up, HandleUtility.GetHandleSize(cursorPosition) * 0.1f);

            var projected = script.GetClosestPoint(cursorPosition, out float progress);

            Handles.color = Color.green * 0.9f;
            Handles.DrawSolidDisc(projected.position, Vector3.up, HandleUtility.GetHandleSize(projected.position) * 0.1f);


            LineBehavior2.PointData p = null;
            if(script.Points.Count > 0)
            {
                p = script.Points.FindClosest(projected.position, x => x.position, out float _);
                Handles.color = Color.red * 0.9f;
                Handles.DrawSolidDisc(p.position, Vector3.up, HandleUtility.GetHandleSize(projected.position) * 0.1f);
            }

            if(Event.current.type == EventType.MouseDown)
            {
                if(Event.current.button == 0)
                {
                    Undo.RecordObject(script, "Insert Point");
                    script.InsertPointAt(progress, cursorPosition);
                    EditorUtility.SetDirty(script);

                    Event.current.Use();
                }
                else if (Event.current.button == 1 && p != null)
                {
                    Undo.RecordObject(script, "Remove point");
                    script.Points.Remove(p);
                    EditorUtility.SetDirty(script);

                    Event.current.Use();
                }
            }
        }

        private void RotationTool()
        {
            for (int i = 0; i < script.Points.Count; i++)
            {
                Quaternion rot = script.Points[i].rotation;
                Quaternion newPoint = Handles.RotationHandle(rot, script.Points[i].position);

                script.Points[i].rotation = newPoint;
            }
        }

        private void PositioningTool()
        {
            Vector3 cursorPosition = Vector3.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                cursorPosition = ray.GetPoint(distance);
            }

            Handles.color = Color.blue;
            for (int i = 0; i < script.Points.Count; i++)
            {
                LineBehavior2.PointData point = script.Points[i];
                Vector3 newPoint = Vector3.zero;
                    
                if(false)
                {
                    newPoint = Handles.FreeMoveHandle(point.position, HandleUtility.GetHandleSize(point.position) * 0.1f, Vector3.one, Handles.RectangleHandleCap);
                    if (newPoint != point.position)
                    {
                        script.Points[i].position = cursorPosition;
                    }
                }
                else
                {

                    newPoint = Handles.PositionHandle(point.position, Quaternion.identity);
                    if (newPoint != point.position)
                    {
                        Undo.RecordObject(script, "Move Point");
                        script.Points[i].position = newPoint;
                        EditorUtility.SetDirty(script);
                    }
                }
                    

            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mode:");
            if(GUILayout.Button($"{currentMode}"))
            {
                // Cycle through enum modes
                currentMode = (Mode)(((int)currentMode + 1) % Enum.GetValues(typeof(Mode)).Length);
            }
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Set Rotations to Slope"))
            {
                script.SetRotationsToSlope();
            }

            if(GUILayout.Button("Reverse Points"))
            {
                script.Points.Reverse();
            }


        }
    }
#endif
}
