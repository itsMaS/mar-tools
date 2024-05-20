namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EdgeFillLineBehavior : LineBehaviorReceiver
    {
        public float distanceBetweenObjects = 1f;
        public float placementOffset = 0f;
        public float sampleOffset = 0;
        public GameObject prefab;

        public override void UpdateEditor()
        {
            base.UpdateEditor();

            if (!prefab) return;
            ClearElements();

            var points = line.GetPointsAlongLine(distanceBetweenObjects, sampleOffset).
                ConvertAll(x => (x.Item1 + Vector3.Cross(x.Item2, Vector3.up).normalized * placementOffset, x.Item2));

            points = points.GroupBy(x => x.Item1 + x.Item2).Where(g => g.Count() < 2).SelectMany(g => g).ToList();
            
            foreach (var item in points)
            {
                var el = AddElement(prefab, item.Item1, Quaternion.LookRotation(item.Item2, Vector3.up));
            }
        }
    }
} 
