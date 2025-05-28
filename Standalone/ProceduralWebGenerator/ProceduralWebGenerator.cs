using UnityEngine;
using MarTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HauntedPaws
{
    public class ProceduralWebGenerator : MonoBehaviour
    {
        public Material webMaterial;

        public float castSize = 5;
        public float segmentWidth = 0.1f;
        public int beams = 12;
        public float lineWidth = 0.05f;
        public float castDeviationAlongPlane = 0.2f;

        public void Generate()
        {
            if(transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            Vector3[,] horizontalPoints = new Vector3[beams, 99];

            int maxSegments = 0;
            for (int i = 0; i < beams; i++)
            {
                float angle = Mathf.PI * 2f / beams;

                float offset = i + Random.value.Remap01(-castDeviationAlongPlane, castDeviationAlongPlane);

                Vector3 point = new Vector3(Mathf.Cos(offset * angle), 0, Mathf.Sin(offset * angle));
                Vector3 worldPoint = transform.TransformPoint(point);

                Vector3 direction = worldPoint - transform.position;

                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, castSize, 1, QueryTriggerInteraction.Ignore))
                {
                    worldPoint = hit.point;
                    ConnectLines(transform.position, hit.point);

                    int segments = Mathf.FloorToInt(hit.distance / segmentWidth);

                    if(segments > maxSegments)
                    {
                        maxSegments = segments;
                    }

                    for (int j = 0; j < segments; j++)
                    {
                        float interpolator = j / (float)segments;
                        interpolator = interpolator + Random.value.Remap01(-0.01f, 0.01f);

                        Vector3 segmentPoint = Vector3.Lerp(transform.position, hit.point, interpolator);
                        horizontalPoints[i, j] = segmentPoint;
                    }
                }
            }

            for (int i = 0; i < maxSegments; i++)
            {
                for (int j = 1; j < beams; j++)
                {
                    int indexA = j;
                    int indexB = (j + 1) % (beams);

                    if (horizontalPoints[indexA, i] == Vector3.zero || horizontalPoints[indexB, i] == Vector3.zero)
                    {
                        continue;
                    }

                    ConnectLines(horizontalPoints[indexA, i], horizontalPoints[indexB, i]);
                }
            }
        }

        public void ConnectLines(Vector3 p1, Vector3 p2)
        {
            GameObject parent;
            if(transform.childCount <= 0)
            {
                parent = new GameObject("Web");
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
                parent.transform.localRotation = Quaternion.identity;
            }
            else
            {
                parent = transform.GetChild(0).gameObject;
            }

            GameObject line = new GameObject("line");
            LineRenderer lr = line.AddComponent<LineRenderer>();

            line.transform.SetParent(parent.transform);
            line.transform.localPosition = Vector3.zero;
            line.transform.localRotation = Quaternion.identity;

            lr.positionCount = 2;
            lr.SetPosition(0, p1);
            lr.SetPosition(1, p2);

            lr.material = webMaterial;

            lr.widthMultiplier = lineWidth;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ProceduralWebGenerator))]
    [CanEditMultipleObjects]
    public class ProceduralWebGeneratorEditor : MarToolsEditor<ProceduralWebGenerator>
    {
        private void OnSceneGUI()
        {
            Handles.color = Color.white.SetAlpha(0.1f);
            Handles.DrawSolidDisc(script.transform.position, script.transform.up, script.castSize);

            int beams = 20;
            for (int i = 0; i < beams; i++)
            {
                float angle = Mathf.PI * 2f / beams;
                Vector3 point = new Vector3(Mathf.Cos(i * angle), 0, Mathf.Sin(i * angle));
                Vector3 worldPoint = script.transform.TransformPoint(point);

                Vector3 direction = worldPoint - script.transform.position;
                direction = direction.RandomDeviation(2);

                if (Physics.Raycast(script.transform.position, direction, out RaycastHit hit, script.castSize, 1, QueryTriggerInteraction.Ignore))
                {
                    worldPoint = hit.point;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Generate"))
            {
                foreach (var item in scripts)
                {
                    item.Generate();
                }
            }
        }
    }
#endif
}
