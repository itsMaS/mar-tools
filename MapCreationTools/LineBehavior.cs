using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class LineBehavior : MonoBehaviour
{
    [SerializeField]
    public float lineWidth = 1;

    [SerializeField]
    public Color lineColor = Color.white;

    [SerializeField]
    public int smoothing = 5;

    [SerializeField]
    public bool looping = false;

    [SerializeField]
    public List<Vector3> points = new List<Vector3>();

    public List<Vector3> worldPoints
    {
        get
        {
            return points.ConvertAll(p => transform.TransformPoint(p));
        }
    }

    public List<Vector3> smoothPoints
    {
        get
        {
            return GenerateSmoothPath(worldPoints, smoothing);
        }
    }

    public List<Vector3> GetPointsInsideShape(int pointCount, int seed)
    {
        return Utilities.GetPointsInsideShape(worldPoints, pointCount, seed);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        List<Vector3> points = smoothPoints;

        //Handles.color = lineColor;
        //Handles.DrawAAPolyLine(lineWidth, points.ToArray());

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 current = points[i];
            Vector3 previous = points[i-1];

            Gizmos.DrawLine(current, previous);
        }
    }

    public float GetLength()
    {
        List<Vector3> points = smoothPoints;

        float length = 0;
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 current = points[i];
            Vector3 previous = points[i - 1];

            length += Vector3.Distance(current, previous);
        }
        return length;
    }

    public List<Vector3> GenerateSmoothPath(List<Vector3> cpoints, int smoothness)
    {
        if (cpoints.Count == 0) return cpoints;

        var controlPoints = new List<Vector3>(cpoints);

        if(smoothing <= 0)
        {
            if(looping)
                controlPoints.Add(cpoints[0]);
            return controlPoints;
        }

        // Ensure that we can loop the points by adding the first two points to the end of the list

        if(looping)
        {
            controlPoints.Add(cpoints.First());
            controlPoints.Insert(0, cpoints.Last());
            controlPoints.Insert(0, cpoints.Last());
        }
        else
        {
            controlPoints.Add(cpoints.Last());
            controlPoints.Insert(0, cpoints.First());
        }


        List<Vector3> smoothPoints = new List<Vector3>();
        if (controlPoints.Count < 2)
            return smoothPoints; // Not enough points to create a smooth path.

        for (int index = 0; index < controlPoints.Count - 3; index++)
        {
            Vector3 p0 = controlPoints[index];
            Vector3 p1 = controlPoints[index + 1];
            Vector3 p2 = controlPoints[index + 2];
            Vector3 p3 = controlPoints[index + 3];

            for (int i = 0; i <= smoothness; i++)
            {
                float t = i / (float)smoothness;
                float t2 = t * t;
                float t3 = t2 * t;
                Vector3 position = 0.5f *
                    ((2.0f * p1) +
                    (-p0 + p2) * t +
                    (2.0f * p0 - 5.0f * p1 + 4f * p2 - p3) * t2 +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                smoothPoints.Add(position);
            }
        }
        return smoothPoints;
    }
}

[CustomEditor(typeof(LineBehavior))]
public class LineDrawerEditor : Editor
{
    private LineBehavior lineDrawer;
    private Vector3 cursorWorldPosition;

    private float gridSize = 5;
    private bool snap = false;


    private void OnEnable()
    {
        lineDrawer = target as LineBehavior;
        EditorApplication.update += UpdateEditor;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Point Settings", EditorStyles.boldLabel);
        lineDrawer.lineWidth = EditorGUILayout.FloatField("Width", lineDrawer.lineWidth);
        lineDrawer.lineColor = EditorGUILayout.ColorField("Color", lineDrawer.lineColor);
        lineDrawer.smoothing = EditorGUILayout.IntSlider("Smoothing", lineDrawer.smoothing, 0, 10);
        lineDrawer.looping = EditorGUILayout.Toggle("Looping", lineDrawer.looping);

        gridSize = EditorGUILayout.FloatField("Grid size", gridSize);

        if(GUILayout.Button("Clear Points"))
        {
            lineDrawer.points.Clear();
        }
        if (GUILayout.Button("Snap points to grid"))
        {
            for (int i = 0; i < lineDrawer.points.Count; i++)
            {
                lineDrawer.points[i] = lineDrawer.points[i].Round(gridSize);
            }
        }
        if (GUILayout.Button("Set pivot to median points"))
        {
            var worldPositions = lineDrawer.worldPoints;

            Vector3 average = Vector3.zero;
            foreach (var item in worldPositions)
            {
                average += item;
            }

            average /= worldPositions.Count;

            lineDrawer.transform.position = average;

            lineDrawer.points = worldPositions.ConvertAll(p => lineDrawer.transform.InverseTransformPoint(p));
        }
    }

    private void OnSceneGUI()
    {
        Plane plane = new Plane(Vector3.up, 0);
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if(plane.Raycast(ray, out float distance))
        {
            cursorWorldPosition = ray.GetPoint(distance);
        }

        if (Event.current.control && Event.current.type == EventType.KeyDown)
        {
            snap = !snap;
        }

        if(snap)
        {
            int amount = 30;
            float length = amount * gridSize * 2;
            for (int i = -amount; i < amount; i++)
            {
                Vector3 pointVertical = lineDrawer.transform.TransformPoint(new Vector3(i * gridSize, 0, -length / 2));
                Vector3 pointHorizontal = lineDrawer.transform.TransformPoint(new Vector3(-length / 2, 0, i * gridSize));

                Handles.color = Color.white * (0.4f + (((i + 10000) % 10) == 0 ? 0.2f : 0));
                Handles.DrawLine(pointVertical, pointVertical + lineDrawer.transform.forward * length);
                Handles.DrawLine(pointHorizontal, pointHorizontal + lineDrawer.transform.right * length);
            }
        }

        AddPoints();

        var localPoints = lineDrawer.worldPoints;
        Color col = lineDrawer.lineColor;
        col.a = 1;
        Handles.color = col;

        Handles.DrawAAPolyLine(2, localPoints.ToArray());
    }

    private void UpdateEditor()
    {
        SceneView.RepaintAll();
    }

    private void AddPoints()
    {
        if (Event.current.shift)
        {
            var localPoints = lineDrawer.worldPoints;


            float minDistanceToLine = float.MaxValue;
            int minIndex1 = 0;
            int minIndex2 = 0;
            
            for (int i = 0; i < localPoints.Count; i++)
            {
                int ind1 = i;
                int ind2 = (i + 1) % localPoints.Count;

                Vector3 point1 = localPoints[ind1];
                Vector3 point2 = localPoints[ind2];

                float dist = DistancePointToLineSegment(cursorWorldPosition, point1, point2);

                if(dist < minDistanceToLine)
                {
                    minDistanceToLine = dist;
                    minIndex1 = ind1;
                    minIndex2 = ind2;
                }
            }

            int insertIndex = Mathf.Max(minIndex1, minIndex2);
            if(Mathf.Min(minIndex1, minIndex2) == 0)
            {
                insertIndex = 0;
            }

            int removeIndex = 0;

            if(localPoints.Count > 0)
            {
                float minDistance = localPoints.Min(item => Vector3.Distance(cursorWorldPosition, item));
                removeIndex = localPoints.FindIndex(item => Vector3.Distance(cursorWorldPosition, item) == minDistance);
            }


            if(localPoints.Count > 0 && minIndex2 >= 0 && minIndex1 >= 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(localPoints[minIndex1], Vector3.up, 1);
                Handles.DrawWireDisc(localPoints[minIndex2], Vector3.up, 1);
            }

            Handles.color = Color.red;

            if(localPoints.Count > 0)
            {
                Handles.DrawWireDisc(localPoints[removeIndex], Vector3.up, 0.5f);
            }

            Handles.color = Color.white;
            Handles.DrawWireDisc(cursorWorldPosition, Vector3.up, 0.5f);
            if(Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    lineDrawer.points.Insert(insertIndex, lineDrawer.transform.InverseTransformPoint(cursorWorldPosition));
                    Event.current.Use();
                }
                else if (Event.current.button == 1)
                {
                    lineDrawer.points.RemoveAt(removeIndex);
                }
            }

            for (int i = 0; i < localPoints.Count; i++)
            {
                Vector3 pos = localPoints[i];
                Handles.Label(pos, $"{i}");
            }

        }
        else
        {
            List<Vector3> localPoints = lineDrawer.worldPoints;
            for (int i = 0; i < localPoints.Count; i++)
            {
                Vector3 oldPoint = localPoints[i];

                EditorGUI.BeginChangeCheck();

                Handles.color = Color.blue;
                Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, HandleUtility.GetHandleSize(oldPoint) * 0.1f, Vector3.one, Handles.RectangleHandleCap);
                Vector3 newPointLocal = lineDrawer.transform.InverseTransformPoint(newPoint);

                if (EditorGUI.EndChangeCheck())
                {
                    if(snap)
                    {

                        newPointLocal = newPointLocal.Round(gridSize);
                    }
                }

                lineDrawer.points[i] = newPointLocal;
            }
        }
    }

    // Method to calculate the minimum distance from a point to a line segment
    public float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        // Vector from start of line to point
        Vector3 lineToPoint = point - lineStart;

        // Vector from start of line to end of line
        Vector3 lineVector = lineEnd - lineStart;

        // Project point onto the line:
        // projectionFactor determines where the projected point lies relative to lineStart and lineEnd
        float lineLengthSquared = lineVector.sqrMagnitude; // Length squared of the line segment
        float projectionFactor = Vector3.Dot(lineToPoint, lineVector) / lineLengthSquared;

        // Check if the projection of the point is actually on the line segment
        if (projectionFactor < 0)
        {
            // Closest to lineStart
            return Vector3.Distance(point, lineStart);
        }
        else if (projectionFactor > 1)
        {
            // Closest to lineEnd
            return Vector3.Distance(point, lineEnd);
        }
        else
        {
            // Projection point is on the segment
            Vector3 nearestPointOnLine = lineStart + projectionFactor * lineVector;
            return Vector3.Distance(point, nearestPointOnLine);
        }
    }
}
