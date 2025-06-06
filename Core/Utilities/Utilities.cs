namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UI;

    public static class Utilities
    {
        /// <summary>
        /// Returns the closest point on a line segment defined by two endpoints.
        /// </summary>
        /// <param name="point">The point to project onto the segment.</param>
        /// <param name="segmentStart">The start point of the segment.</param>
        /// <param name="segmentEnd">The end point of the segment.</param>
        /// <returns>The closest point on the segment to the given point.</returns>
        public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd, out float progress)
        {
            Vector3 segmentVector = segmentEnd - segmentStart;
            // Calculate the projection parameter t along the segment.
            float t = Vector3.Dot(point - segmentStart, segmentVector) / segmentVector.sqrMagnitude;
            // Clamp t to the [0, 1] range so that the point lies on the segment.
            progress = Mathf.Clamp01(t);
            return segmentStart + segmentVector * progress;
        }


        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Coroutine DelayedAction(this MonoBehaviour behavior, float time, Action onComplete, Action<float> onUpdate = null, bool timeScaled = true, Ease ease = Ease.InOutQuad, AnimationCurve curve = null)
        {
            if (behavior.enabled && behavior.gameObject.activeInHierarchy)
                return behavior.StartCoroutine(DelayedCoroutine(time, onComplete, onUpdate, timeScaled, ease, curve));
            else
                return null;
        }

        public static Coroutine MoveToTween(this MonoBehaviour behavior, Transform target, float duration, Vector3 targetPosition, Quaternion targetRotation, Action onComplete = null, bool timesScaled = true, Ease ease = Ease.InOutQuad)
        {
            Vector3 startPosition = target.position;
            Quaternion startRotation = target.rotation;

            return behavior.DelayedAction(duration, () =>
            {
                target.position = targetPosition;
                target.rotation = targetRotation;
                onComplete?.Invoke();
            }, t =>
            {
                target.position = Vector3.Lerp(startPosition, targetPosition, t);
                target.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            });
        }

        private static IEnumerator DelayedCoroutine(float duration, Action onComplete, Action<float> onUpdate, bool timeScaled = true, Ease ease = Ease.InOutSine, AnimationCurve curve = null)
        {
            float elapsed = 0;
            float t = 0;
            while (elapsed < duration)
            {
                elapsed += timeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                t = elapsed / duration;

                float adjustedT = 0;

                if (ease != Ease.Custom)
                {
                    adjustedT = Eases[ease].Invoke(t);
                }
                else
                {
                    adjustedT = curve.Evaluate(t);
                }

                onUpdate?.Invoke(adjustedT);
                yield return null;
            }

            if (ease != Ease.Custom)
            {
                onUpdate?.Invoke(Eases[ease].Invoke(1));
            }
            else
            {
                onUpdate?.Invoke(curve.Evaluate(t));
            }
            onComplete?.Invoke();
        }

        public static T FindClosest<T>(this IEnumerable<T> collection, Vector3 target, Func<T, Vector3> PositionFunction, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            T closestElement = collection.First();

            foreach (var item in collection)
            {
                float distance = Vector3.Distance(PositionFunction.Invoke(item), target);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestElement = item;
                }
            }

            return closestElement;
        }


        public static bool IsInAnyOfTheLayers(this GameObject go, LayerMask layers)
        {
            return ((1 << go.layer) & layers.value) != 0;
        }
        public static bool IsInAnyOfTheLayers(this GameObject go, params string[] layers)
        {
            return IsInAnyOfTheLayers(go, LayerMask.GetMask(layers));
        }

        public static float Remap(this float input, Vector2 from, Vector2 to)
        {
            return Remap(input, from.x, from.y, to.x, to.y);
        }

        public static float Distance(this float input, float destination)
        {
            return Mathf.Abs(destination - input);
        }

        public static float Remap(this float input, float i1, float i2, float o1 = 0, float o2 = 1, bool clamp = true)
        {
            float min = Mathf.Min(o1, o2);
            float max = Mathf.Max(o1, o2);

            float result = Mathf.LerpUnclamped(o1, o2, Mathf.InverseLerp(i1, i2, input));

            if (!clamp) return result;
            return Mathf.Clamp(result, min, max);
        }

        public static float Remap01(this float input, float o1, float o2, bool clamped = true)
        {
            return Remap(input, 0, 1, o1, o2, clamped);
        }

        public static Vector2 FindClosest(this IEnumerable<Vector2> collection, Vector2 target, out float closestDistance)
        {
            return FindClosest<Vector2>(collection, target, item => item, out closestDistance);
        }

        public static Vector3 Right(this Quaternion rotation)
        {
            return rotation * Vector3.right;
        }
        public static Vector3 Forward(this Quaternion rotation)
        {
            return rotation * Vector3.forward;
        }
        public static Vector3 Up(this Quaternion rotation)
        {
            return rotation * Vector3.up;
        }

        public static Vector3 RandomOffset(this Vector3 origin, float amplitude)
        {
            return Vector3.zero;
        }

        public static float AbsMax(float val1, float val2)
        {
            return Mathf.Abs(val1) >= Mathf.Abs(val2) ? val1 : val2;
        }

        public enum Ease
        {
            Linear,
            InOutSine,
            InOutQuad,
            OutBack,
            Pulse,
            OutBounce,
            OutQuad,
            InQuad,
            Custom = 99,
        }
        
        public static Dictionary<Ease, Func<float, float>> Eases = new Dictionary<Ease, Func<float, float>>
        {
            {Ease.Linear, t1 => Linear(t1)},
            {Ease.InOutSine, t1 => InOutSine(t1)},
            {Ease.InOutQuad, t1 => InOutQuad(t1)},
            {Ease.OutBack, t1 => OutBack(t1)},
            {Ease.Pulse, t1 => Pulse(t1)},
            {Ease.OutBounce, t1 => OutBounce(t1)},
            {Ease.OutQuad, t1 => OutQuad(t1) },
            {Ease.InQuad, t1 => InQuad(t1) },
        };

        public static float EaseGravity(float t, float start, float peak, float end)
        {
            // Quadratic gravity-based trajectory easing
            float midpoint = 0.5f; // The peak happens at 50% progress
            if (t < midpoint)
            {
                // First half: projectile moves up
                return Mathf.Lerp(start, peak, t / midpoint);
            }
            else
            {
                // Second half: projectile moves down
                return Mathf.Lerp(peak, end, (t - midpoint) / (1f - midpoint));
            }
        }

        private static float InQuad(float t)
        {
            return t * t;
        }

        private static float OutQuad(float t)
        {
            return t * (2 - t);
        }

        public static float Linear(float t)
        {
            return t;
        }
        public static float InOutSine(float t)
        {
            return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1);
        }
        public static float InOutQuad(float t)
        {
            t *= 2;
            if (t < 1) return 0.5f * t * t;
            t--;
            return -0.5f * (t * (t - 2) - 1);
        }
        public static float OutBack(float t)
        {
            float s = 1.70158f;  // Overshoot amount, can be adjusted
            return ((t -= 1) * t * ((s + 1) * t + s) + 1);
        }
        
        public static float Pulse(float t)
        {
            return Mathf.Sin(t * Mathf.PI);
        }
        
        public static int LayerMaskToLayer(LayerMask layerMask)
        {
            int layerNumber = -1;
            int mask = layerMask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    layerNumber = i;
                    break;
                }
            }
            return layerNumber;
        }
        public static float OutBounce(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5 / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
        
        public static Vector3 MaskY(this Vector3 vector, float yValue = 0)
        {
            vector.y = yValue;
            return vector;
        }
        
        public static Vector3 Snap(this Vector3 vector, float gridScale)
        {
            vector /= gridScale;
            vector = new Vector3(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
            vector *= gridScale;
        
            return vector;
        }
        
        public static float Round(this float value, float gridSize)
        {
            value /= gridSize;
            value = Mathf.RoundToInt(value);
            value *= gridSize;
        
            return value;
        }
        
        public static List<Vector3> GetPointsInsideShape(List<Vector3> vertices, int pointCount, int seed)
        {
            List<Vector3> pointsInside = new List<Vector3>();
            Bounds bounds = CalculateBounds(vertices);
            int safetyNet = 0;
        
            UnityEngine.Random.InitState(seed);
        
            while (pointsInside.Count < pointCount)
            {
                Vector3 randomPoint = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    0, // All points are at y = 0 as it's the XZ plane
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
        
                if (IsPointInside(vertices, randomPoint))
                {
                    pointsInside.Add(randomPoint);
                }
        
                safetyNet++;
                if (safetyNet > 10000) // Prevent infinite loop
                {
                    Debug.LogWarning("Too many attempts to find points inside the shape.");
                    break;
                }
            }
            return pointsInside;
        }
        
        public static bool IsPointInside(List<Vector3> vertices, Vector3 point)
        {
            bool inside = false;
            int j = vertices.Count - 1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (((vertices[i].z > point.z) != (vertices[j].z > point.z)) &&
                    (point.x < (vertices[j].x - vertices[i].x) * (point.z - vertices[i].z) / (vertices[j].z - vertices[i].z) + vertices[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }
        
        private static Bounds CalculateBounds(List<Vector3> vertices)
        {
            Vector3 min = vertices[0];
            Vector3 max = vertices[0];
            foreach (Vector3 vertex in vertices)
            {
                min.x = Mathf.Min(min.x, vertex.x);
                min.z = Mathf.Min(min.z, vertex.z);
                max.x = Mathf.Max(max.x, vertex.x);
                max.z = Mathf.Max(max.z, vertex.z);
            }
            return new Bounds((max + min) / 2, max - min);
        }
        
        public static float CalculateLength(this IEnumerable<Vector3> points)
        {
            float distance = 0;
            if (points.Count() == 0) return distance;
        
            Vector3 last = points.First();
            foreach (Vector3 point in points)
            {
                float dis = Vector3.Distance(last, point);
                distance += dis;
                last = point;
            }
        
            return distance;
        }

        /// <summary>
        /// Gets positions and normals along a path.
        /// </summary>
        /// <param name="pathPoints">A list of Vector3 points defining the path.</param>
        /// <param name="numPoints">The desired number of output points and normals.</param>
        public static (List<Vector3>, List<Vector3>) GetPositionsAndNormals(List<Vector3> pathPoints, int numPoints)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();

            if (pathPoints == null || pathPoints.Count < 2 || numPoints <= 0)
            {
                Debug.LogWarning("Invalid input data provided for path calculation.");
                return (positions, normals);
            }

            // Calculate total path length
            float totalLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
                segmentLengths.Add(segmentLength);
                totalLength += segmentLength;
            }

            // Distribute the desired points evenly along the path
            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)(numPoints);
                float targetDistance = t * totalLength;
                float accumulatedDistance = 0f;

                // Find the segment containing the target distance
                int segmentIndex = 0;
                while (segmentIndex < segmentLengths.Count && accumulatedDistance + segmentLengths[segmentIndex] < targetDistance)
                {
                    accumulatedDistance += segmentLengths[segmentIndex];
                    segmentIndex++;
                }

                // Interpolate within the identified segment
                if (segmentIndex < segmentLengths.Count)
                {
                    float remainingDistance = targetDistance - accumulatedDistance;
                    float segmentT = remainingDistance / segmentLengths[segmentIndex];

                    Vector3 segmentStart = pathPoints[segmentIndex];
                    Vector3 segmentEnd = pathPoints[segmentIndex + 1];
                    Vector3 interpolatedPosition = Vector3.Lerp(segmentStart, segmentEnd, segmentT);
                    positions.Add(interpolatedPosition);

                    // Calculate tangent and then normal vector
                    Vector3 tangent = (segmentEnd - segmentStart).normalized;
                    Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
                    normals.Add(normal);
                }
            }

            return (positions, normals);
        }

        public static List<(Vector3, Vector3)> GetPositionsAndNormals(List<Vector3> pathPoints, float distance, float offset)
        {
            List<(Vector3, Vector3)> positions = new List<(Vector3, Vector3)>();

            int numPoints = Mathf.CeilToInt(pathPoints.CalculateLength() / distance);

            if (pathPoints == null || pathPoints.Count < 2 || numPoints <= 0)
            {
                return positions;
            }

            // Calculate total path length
            float totalLength = 0;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
                segmentLengths.Add(segmentLength);
                totalLength += segmentLength;
            }

            // Distribute the desired points evenly along the path
            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)(numPoints);
                float targetDistance = t * totalLength;
                float accumulatedDistance = offset;

                // Find the segment containing the target distance
                int segmentIndex = 0;
                while (segmentIndex < segmentLengths.Count && accumulatedDistance + segmentLengths[segmentIndex] < targetDistance)
                {
                    accumulatedDistance += segmentLengths[segmentIndex];
                    segmentIndex++;
                }

                // Interpolate within the identified segment
                if (segmentIndex < segmentLengths.Count)
                {
                    float remainingDistance = targetDistance - accumulatedDistance;
                    float segmentT = remainingDistance / segmentLengths[segmentIndex];

                    Vector3 segmentStart = pathPoints[segmentIndex];
                    Vector3 segmentEnd = pathPoints[segmentIndex + 1];
                    Vector3 interpolatedPosition = Vector3.Lerp(segmentStart, segmentEnd, segmentT);

                    // Calculate tangent and then normal vector
                    Vector3 tangent = (segmentEnd - segmentStart).normalized;
                    Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
                    positions.Add((interpolatedPosition, normal));
                }
            }

            return (positions);
        }

        public static Transform FindRecursive(this Transform parent, string childName, bool includeDisabled = false)
        {
            if (parent.name == childName)
                return parent;

            foreach (Transform child in parent)
            {
                if (!includeDisabled && !child.gameObject.activeInHierarchy) continue;

                Transform result = child.FindRecursive(childName, includeDisabled);
                if (result != null)
                    return result;
            }

            return null;
        }

        // This method assumes that path is a list of points forming a polyline
        // and t is the normalized position along the path (0=start, 1=end)
        public static (Vector3 point, (Vector3 forward, Vector3 right)) GetPointAndNormalAlongPath(this List<Vector3> path, float t)
        {
            if (path == null || path.Count < 2)
            {
                Debug.LogError("Invalid path data.");
                return (Vector3.zero, (Vector3.forward, Vector3.right)); // Return up vector as default normal in error case
            }

            // Clamp t to be between 0 and 1
            t = Mathf.Clamp01(t);

            // Calculate the total length of the path and the lengths between each point
            float totalLength = 0;
            List<float> lengths = new List<float>();

            for (int i = 1; i < path.Count; i++)
            {
                float segmentLength = Vector3.Distance(path[i - 1], path[i]);
                lengths.Add(segmentLength);
                totalLength += segmentLength;
            }

            // Find the segment where the point should be
            float targetLength = t * totalLength;
            float accumulatedLength = 0;

            for (int i = 0; i < lengths.Count; i++)
            {
                accumulatedLength += lengths[i];
                if (accumulatedLength >= targetLength)
                {
                    // Found the segment
                    float segmentT = 1 - (accumulatedLength - targetLength) / lengths[i];
                    Vector3 pointOnSegment = Vector3.Lerp(path[i], path[i + 1], segmentT);

                    // Calculate the tangent vector as the derivative of the Lerp
                    Vector3 tangent = (path[i + 1] - path[i]).normalized;

                    // Generate a simple normal (not necessarily the true normal, depending on the path orientation)
                    // Assuming the path is generally horizontal, a perpendicular vector in the plane can be a normal
                    Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;

                    return (pointOnSegment, (tangent, normal));
                }
            }

            // If we're at the end of the path
            return (path[path.Count - 1], (Vector3.forward, Vector3.right));
        }

        public static Vector2 Project(Vector2 a, Vector2 b)
        {
            float dotProduct = Vector2.Dot(a, b);
            float magnitudeB = b.sqrMagnitude;
            return (dotProduct / magnitudeB) * b;
        }

        // Method to rotate a Vector2 by a specified angle in degrees
        public static Vector2 Rotate(this Vector2 v, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad; // Convert the angle to radians
            float cos = Mathf.Cos(angleRadians);
            float sin = Mathf.Sin(angleRadians);

            float newX = v.x * cos - v.y * sin;
            float newY = v.x * sin + v.y * cos;

            return new Vector2(newX, newY);
        }


        public static List<T> GetVariablesOfType<T>(this Component comp, bool includeDerived = false)
        {
            return GetFieldsOfType<T>(comp, includeDerived).ConvertAll(x => (T)x.GetValue(comp));
        }

        public static List<FieldInfo> GetFieldsOfType<T>(this Component comp, bool includeDerived = false)
        {
            if (comp == null)
                return null; // or throw an ArgumentNullException if that's preferable

            List<FieldInfo> matchingFields = new List<FieldInfo>();
            // Get all public instance fields of the component
            FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (includeDerived ? typeof(T).IsAssignableFrom(field.FieldType) : field.FieldType == typeof(T))
                {
                    matchingFields.Add(field);
                }
            }

            return matchingFields;
        }

        // Extension method to get a deviated Vector3 within a specified angle
        public static Vector3 RandomDeviation(this Vector3 original, float maxAngle)
        {
            // Create a random rotation quaternion within the specified max angle around a random axis
            Quaternion randomRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, maxAngle), UnityEngine.Random.onUnitSphere);

            // Apply the rotation to the original vector
            return randomRotation * original;
        }

        public static bool TryGetComponentInParent<T>(this GameObject go, out T component)
        {
            component = go.GetComponentInParent<T>();
            return component != null;
        }

        public static List<Vector3> GenerateSmoothPath(this List<Vector3> cpoints, float smoothingLength = 1, int smoothness = 5, bool looping = false)
        {
            if (cpoints.Count == 0) return cpoints;

            var controlPoints = new List<Vector3>(cpoints);

            if (smoothness <= 0)
            {
                if (looping)
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

                if (!looping)
                {
                    if (i == 0 || i == controlPoints.Count - 1) n = Vector3.zero;
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
                    float t = (float)j / (smoothness - 1);

                    smoothPoints.Add(BezierCurve(t, p1, tan1.Item2, tan2.Item1, p2));
                }
            }

            return smoothPoints;
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

        public static List<Transform> GetTransformChain(Transform ancestor, Transform descendant)
        {
            if (!descendant.IsChildOf(ancestor))
                return null; // No valid hierarchy connection

            List<Transform> chain = new List<Transform>();

            Transform current = descendant;
            while (current != null)
            {
                chain.Add(current);
                if (current == ancestor)
                    break;
                current = current.parent;
            }

            chain.Reverse(); // Reverse to get top-down order
            return chain;
        }

        public static Texture2D GenerateGradientTexture(Gradient gradient, int width = 256, int height = 1)
        {
            if (gradient == null)
            {
                Debug.LogError("Gradient is null!");
                return null;
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < width; x++)
            {
                Color color = gradient.Evaluate((float)x / (width - 1));
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Checks whether this GameObject is in the DontDestroyOnLoad scene.
        /// </summary>
        /// <param name="go">The GameObject to test.</param>
        /// <returns>True if the GameObject is in DontDestroyOnLoad; otherwise false.</returns>
        public static bool IsInDontDestroyOnLoad(this GameObject go)
        {
            // In modern Unity versions (5.4+), objects under DontDestroyOnLoad
            // are placed in a special scene named "DontDestroyOnLoad".
            return go.scene.name == "DontDestroyOnLoad";
        }
    }
}

