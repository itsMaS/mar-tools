namespace MarTools
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UIElements;

    [SelectionBase]
    public class LineBehavior : MonoBehaviour
    {
        public UnityEvent OnModified;

        public bool autoUpdate = true;
        [SerializeField]
        public Color lineColor = Color.white;
        [SerializeField]
        public int smoothing = 5;
        [SerializeField]
        public float smoothingLength = 1;
        [SerializeField]
        public bool looping = false;
        [SerializeField]
        public List<Vector3> points = new List<Vector3>();

        public List<Vector3> worldPoints { get { return points.ConvertAll(p => transform.TransformPoint(p)); } }
        public List<Vector3> smoothWorldPoints { get { return GenerateSmoothPath(worldPoints, smoothing); } }
        public List<Vector3> GetPointsInsideShape(int pointCount, int seed)
        {
            return Utilities.GetPointsInsideShape(worldPoints, pointCount, seed);
        }
        public float CalculateLength()
        {
            return smoothWorldPoints.CalculateLength();
        }

        public List<(Vector3, Vector3)> GetPointsAlongLine(float distanceBetweenPoints, float offset)
        {
            return Utilities.GetPositionsAndNormals(smoothWorldPoints, distanceBetweenPoints, offset);
        }

        public (Vector3, (Vector3, Vector3)) GetPositionAndNormalAt(float t)
        {
            return smoothWorldPoints.GetPointAndNormalAlongPath(t);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = lineColor;
            List<Vector3> points = smoothWorldPoints;
    
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 current = points[i];
                Vector3 previous = points[i-1];
    
                Gizmos.DrawLine(current, previous);
            }
        }
    
        private List<Vector3> GenerateSmoothPath(List<Vector3> cpoints, int smoothness)
        {
            if (cpoints.Count == 0) return cpoints;
    
            var controlPoints = new List<Vector3>(cpoints);
    
            if(smoothing <= 0)
            {
                if(looping)
                    controlPoints.Add(cpoints[0]);
                return controlPoints;
            }
    
            List<Vector3> smoothPoints = new List<Vector3>();
            if (controlPoints.Count < 2)
                return smoothPoints; // Not enough points to create a smooth path.

            (Vector3, Vector3)[] Tangents = new (Vector3, Vector3)[controlPoints.Count];
            for (int i = 0; i < controlPoints.Count; i++)
            {
                int next = (i + 1) % controlPoints.Count;
                int previous = (i - 1 + controlPoints.Count) % controlPoints.Count;


                Vector3 t1 = (controlPoints[next] - controlPoints[i]);
                Vector3 t2 = (controlPoints[previous] - controlPoints[i]);


                // HACK WITH THE ANGLE TO FLIP TANGENTS
                float angle = Vector3.SignedAngle(t1, t2, Vector3.up);
                Vector3 n = (Vector3.Cross(Vector3.Slerp(t1, t2, 0.5f), angle >= -180 && angle < 0 ? Vector3.up : Vector3.down).normalized * smoothingLength);

                if(!looping)
                {
                    if (i == 0 || i == controlPoints.Count -1) n = Vector3.zero;
                }
                Tangents[i] = (controlPoints[i] + n, controlPoints[i] - n);
            }


            for (int i = 0; i < controlPoints.Count; i++)
            {
                int ind1 = i;
                int ind2 = (i + 1) % controlPoints.Count;

                Vector3 p1 = controlPoints[ind1];
                Vector3 p2 = controlPoints[ind2];

                var tan1 = Tangents[ind1];
                var tan2 = Tangents[ind2];

                if (!looping && i == controlPoints.Count - 1) continue;

                for (int j = 0; j < smoothness; j++)
                {
                    float t = (float)j / (smoothness-1);

                    smoothPoints.Add(BezierCurve(t, p1, tan1.Item2, tan2.Item1, p2));
                }
            }

            return smoothPoints;
        }

        Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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

        internal void Modified()
        {
            OnModified.Invoke();
        }

        internal List<Vector3> GetPointsInsideGrid(float spacing)
        {

            float density = 1f/ spacing;

            List<Vector3> pointsInside = new List<Vector3>();
            List<Vector3> LocalSmoothed = smoothWorldPoints.ConvertAll<Vector3>(item => transform.InverseTransformPoint(item));

            if (LocalSmoothed.Count < 3) return pointsInside;
            Vector4 bounds = new Vector4(LocalSmoothed.Min(i => i.x), LocalSmoothed.Max(i => i.x), LocalSmoothed.Min(i => i.z), LocalSmoothed.Max(i => i.z));
            Vector2 boundsSize = new Vector2(Mathf.Abs(bounds.x - bounds.y), Mathf.Abs(bounds.z - bounds.w));


            int xRows = Mathf.CeilToInt(boundsSize.x * density);
            int yRows = Mathf.CeilToInt(boundsSize.y * density);

            for (int i = 0; i < xRows; i++)
            {
                for (int j = 0; j < yRows; j++)
                {
                    float tHoriz = (float)i / xRows;
                    float tVert = (float)j / yRows;

                    float len = 0.5f / density;
                    Vector3 point = (Mathf.Lerp(bounds.x, bounds.y, tHoriz) + len) * Vector3.right + (Mathf.Lerp(bounds.z, bounds.w, tVert)+ len) * Vector3.forward;
                    if (Utilities.IsPointInside(LocalSmoothed, point))
                    {
                        pointsInside.Add(transform.TransformPoint(point));
                    }
                }
            }

            return pointsInside;
        }

        internal void UpdateShape()
        {
            if (!autoUpdate) return;
            ForceUpdateShape();
        }

        public void ForceUpdateShape()
        {
            foreach (var item in GetComponentsInChildren<LineBehaviorSpawner>())
            {
                item.UpdateShape();
            }

            foreach (var item in GetComponentsInChildren<LineRendererLineBehavior>())
            {
                item.UpdateLine();
            }

            foreach (var item in FindObjectsOfType<LineBehaviorSpawner>().Where(x => x.InsideOf.Contains(this) || x.OutsideOf.Contains(this)))
            {
                item.UpdateShape();
            }
        }

        public bool IsPointInsideShape(Vector3 point)
        {
            return Utilities.IsPointInside(smoothWorldPoints, point);
        }

        public (Vector3, Vector3) GetClosestPointOnLine(Vector3 targetPoint, out int index)
        {
            index = -1;
            List<Vector3> points = smoothWorldPoints;

            if (points.Count < 2)
                throw new InvalidOperationException("The line must have at least two points to calculate a closest point.");

            Vector3 closestPoint = points[0];
            Vector3 normalAtPoint = Vector3.forward;

            float minDistance = float.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 lineStart = points[i];
                Vector3 lineEnd = points[i + 1];

                Vector3 closestPointOnSegment = GetClosestPointOnLineSegment(targetPoint, lineStart, lineEnd);
                float distance = Vector3.Distance(targetPoint, closestPointOnSegment);

                if (distance < minDistance)
                {
                    index = i;
                    minDistance = distance;
                    closestPoint = closestPointOnSegment;
                    normalAtPoint = lineEnd- lineStart;
                }
            }

            return (closestPoint, normalAtPoint);
        }

        private Vector3 GetClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineToPoint = point - lineStart;
            Vector3 lineVector = lineEnd - lineStart;
            float lineLengthSquared = lineVector.sqrMagnitude;

            if (lineLengthSquared == 0.0f)
                return lineStart;

            float projectionFactor = Vector3.Dot(lineToPoint, lineVector) / lineLengthSquared;
            projectionFactor = Mathf.Clamp01(projectionFactor);

            return lineStart + projectionFactor * lineVector;
        }

        public float GetDistanceAlongCurve(Vector3 targetPoint)
        {
            List<Vector3> points = smoothWorldPoints;

            if (points.Count < 2)
                throw new InvalidOperationException("The line must have at least two points to calculate a distance.");

            float totalDistance = 0f;
            float closestDistance = float.MaxValue;
            float distanceAtClosestPoint = 0f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 lineStart = points[i];
                Vector3 lineEnd = points[i + 1];

                // Calculate the distance along the current segment
                float segmentDistance = Vector3.Distance(lineStart, lineEnd);
                totalDistance += segmentDistance;

                // Find the closest point on this segment to the targetPoint
                Vector3 closestPointOnSegment = GetClosestPointOnLineSegment(targetPoint, lineStart, lineEnd);
                float distanceToTarget = Vector3.Distance(targetPoint, closestPointOnSegment);

                // Check if this is the closest point so far
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    distanceAtClosestPoint = totalDistance - segmentDistance + Vector3.Distance(lineStart, closestPointOnSegment);
                }
            }

            return distanceAtClosestPoint;
        }

        public void SetPoints(List<Vector3> Points)
        {
            points = Points.ConvertAll(x => transform.InverseTransformVector(x));
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LineBehavior))]
    public class LineDrawerEditor : Editor
    {
        private LineBehavior lineDrawer;
        private Vector3 cursorWorldPosition;
    
        private float gridSize => EditorPrefs.GetFloat("GridSize", 5);
        private bool snap => EditorPrefs.GetBool("LineBehavior_Snap", false);
        private bool flat => EditorPrefs.GetBool("Flat", true);

        private float lastHeight => lineDrawer.points.Count == 0 ? lineDrawer.transform.position.y : lineDrawer.worldPoints.Last().y;

        private bool editing = false;
        private bool pointsFoldout = false;

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
            lineDrawer.lineColor = EditorGUILayout.ColorField("Color", lineDrawer.lineColor);
            lineDrawer.smoothing = EditorGUILayout.IntSlider("Smoothing", lineDrawer.smoothing, 0, 100);
            lineDrawer.looping = EditorGUILayout.Toggle("Looping", lineDrawer.looping);
            lineDrawer.autoUpdate = EditorGUILayout.Toggle("Auto Update", lineDrawer.autoUpdate);
            lineDrawer.smoothingLength = EditorGUILayout.FloatField("Smoothing length", lineDrawer.smoothingLength);

            if(GUILayout.Button(editing ? "Stop editing" : "Edit"))
            {
                editing = !editing;
            }

            //lastHeight = EditorGUILayout.FloatField("Height", lastHeight);

            bool newFlatSetting = EditorGUILayout.Toggle("Flat", flat);
            EditorPrefs.SetBool("Flat", newFlatSetting);

            bool newSnapSettings = EditorGUILayout.Toggle("Snap", snap);
            EditorPrefs.SetBool("LineBehavior_Snap", newSnapSettings);



            float newGridSize = EditorGUILayout.FloatField("Grid size", gridSize);
            EditorPrefs.SetFloat("GridSize",newGridSize);
    
            if(GUILayout.Button("Clear Points"))
            {
                lineDrawer.points.Clear();
            }
            if (GUILayout.Button("Snap points to grid"))
            {
                for (int i = 0; i < lineDrawer.points.Count; i++)
                {
                    lineDrawer.points[i] = lineDrawer.points[i].Snap(gridSize);
                }
            }
            if(GUILayout.Button("Reverse points"))
            {
                lineDrawer.points.Reverse();
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

            if(GUILayout.Button("Generate"))
            {
                lineDrawer.ForceUpdateShape();
            }

            if(GUILayout.Button("Add point +"))
            {
                Vector3 newPoint = Vector3.zero;
                if(lineDrawer.points.Count > 0)
                {
                    newPoint = lineDrawer.points.Last();
                }

                lineDrawer.points.Add(newPoint + Vector3.up);
            }


            GUILayout.Label("Points");
            pointsFoldout = EditorGUILayout.Foldout(pointsFoldout, "Points List", true);
            if (pointsFoldout)
            {
                for (int i = 0; i < lineDrawer.points.Count; i++)
                {
                    lineDrawer.points[i] = EditorGUILayout.Vector3Field($"P{i}", lineDrawer.points[i]);
                }
            }
        }
    
        private void OnSceneGUI()
        {
            if (!editing) return;
 
            if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                Undo.RecordObject(lineDrawer, "Move points");
                lineDrawer.UpdateShape();
            }

            if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
            {
                EditorPrefs.SetBool("Flat", !flat);
            }

            if(snap)
            {
                DrawGrid();
            }

            AddPoints();
    
            var localPoints = lineDrawer.worldPoints;
            Color col = lineDrawer.lineColor;
            col.a = 1;
            Handles.color = col;
    
            Handles.DrawAAPolyLine(2, localPoints.ToArray());

            Plane plane = new Plane(Vector3.up, -lastHeight);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (plane.Raycast(ray, out float distance))
            {
                cursorWorldPosition = ray.GetPoint(distance);
            }

            foreach (var item in lineDrawer.points)
            {
                Vector3 position = lineDrawer.transform.TransformPoint(item);

                Handles.DrawWireDisc(position, Vector3.up, 0.1f);
                Handles.DrawWireDisc(position.MaskY(0), Vector3.up, 0.1f);
                Handles.DrawDottedLine(position, position.MaskY(0), 2f);
            }


            Vector3 real = cursorWorldPosition;
            Handles.color = Color.white;
            Handles.DrawWireDisc(real, Vector3.up, 0.2f);

            Vector3 ground = real.MaskY(0);
            Handles.color = Color.white * 0.5f;
            Handles.DrawWireDisc(ground, Vector3.up, 0.2f);

            Handles.DrawDottedLine(real, ground, 0.5f);
        }

        private void DrawGrid()
        {
            int amount = 100;
            float length = amount * gridSize * 2;
            for (int i = -amount; i < amount; i++)
            {
                Vector3 pointVertical = lineDrawer.transform.TransformPoint(new Vector3(i * gridSize, 0, -length / 2));
                Vector3 pointHorizontal = lineDrawer.transform.TransformPoint(new Vector3(-length / 2, 0, i * gridSize));

                Handles.color = Color.white * (0.5f + (((i + 10000) % 10) == 0 ? 0.3f : 0.0f));
                Handles.DrawLine(pointVertical, pointVertical + lineDrawer.transform.forward * length);
                Handles.DrawLine(pointHorizontal, pointHorizontal + lineDrawer.transform.right * length);
            }
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
                int removeIndex = 0;

    
                if(localPoints.Count > 0)
                {
                    float minDistance = localPoints.Min(item => Vector3.Distance(cursorWorldPosition, item));
                    removeIndex = localPoints.FindIndex(item => Vector3.Distance(cursorWorldPosition, item) == minDistance);
                }
    
                if(insertIndex == localPoints.Count-1)
                {
                    insertIndex = removeIndex;
                    if (removeIndex == localPoints.Count - 1) insertIndex++;
                }

                float midHeight = 0;
                if(localPoints.Count > 0 && minIndex2 >= 0 && minIndex1 >= 0)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(localPoints[minIndex1], Vector3.up, 1);
                    Handles.DrawWireDisc(localPoints[minIndex2], Vector3.up, 1);
                    midHeight = Mathf.Lerp(localPoints[minIndex1].y, localPoints[minIndex2].y, 0.5f);
                }
    
                Handles.color = Color.red;
    
                if(localPoints.Count > 0)
                {
                    Handles.DrawWireDisc(localPoints[removeIndex], Vector3.up, 0.5f);
                }
    
                Handles.color = Color.white;
                Handles.DrawWireDisc(cursorWorldPosition, Vector3.up, 0.5f);
                Handles.DrawWireDisc(cursorWorldPosition.MaskY(0), Vector3.up, 0.5f);

                Vector3 insertPointWorldCoordinate = Vector3.zero;

                Plane plane = new Plane(Vector3.up, -midHeight);
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (plane.Raycast(ray, out float distance))
                {
                    insertPointWorldCoordinate = ray.GetPoint(distance);
                }



                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        Vector3 insertPoint = lineDrawer.transform.InverseTransformPoint(insertPointWorldCoordinate);
                        if(snap)
                        {
                            insertPoint = insertPoint.Snap(gridSize);
                        }

                        lineDrawer.points.Insert(insertIndex, insertPoint);

                        Event.current.Use();
                        EditorUtility.SetDirty(lineDrawer);

                    }
                    else if (Event.current.button == 1)
                    {
                        lineDrawer.points.RemoveAt(removeIndex);
                        lineDrawer.UpdateShape();

                        Event.current.Use();
                        EditorUtility.SetDirty(lineDrawer);
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
                List<Vector3> worldPoints = lineDrawer.worldPoints;
                for (int i = 0; i < worldPoints.Count; i++)
                {
                    Vector3 oldPoint = worldPoints[i];
    
                    EditorGUI.BeginChangeCheck();
    
                    Handles.color = Color.blue;

                    Vector3 newPoint = Vector3.zero;

                    if(flat)
                    {
                        Handles.FreeMoveHandle(oldPoint, HandleUtility.GetHandleSize(oldPoint) * 0.1f, Vector3.one, Handles.RectangleHandleCap);
                        newPoint = cursorWorldPosition;
                    }
                    else
                    {
                        newPoint = Handles.PositionHandle(oldPoint, Quaternion.identity);
                    }
                    Vector3 newPointLocal = lineDrawer.transform.InverseTransformPoint(newPoint);

                    if (EditorGUI.EndChangeCheck())
                    {

                        newPoint.MaskY(lastHeight);
                        if(snap)
                        {
                            newPointLocal = newPointLocal.Snap(gridSize);
                        }

                        lineDrawer.points[i] = newPointLocal;
                        EditorUtility.SetDirty(lineDrawer);
                    }
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
#endif
}