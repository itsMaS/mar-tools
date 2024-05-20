namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridFillLineBehavior : LineBehaviorReceiver
    {
        public GameObject prefab;
        public float spacing = 2.5f;
        public Vector3 offset;

        public Vector2 randomOffset = Vector2.zero;
        public Vector2 randomYRotationRange = Vector2.zero;
        [Range(0,1)] public float spawnChance = 1;

        public int seed = 0;

        public override void UpdateEditor()
        {
            base.UpdateEditor();

            if (!prefab) return;

            ClearElements();

            Random.InitState(seed);
            foreach (var item in line.GetPointsInsideGrid(spacing))
            {
                if (Random.value > spawnChance) continue;

                var el = AddElement(prefab, item, prefab.transform.rotation);

                Vector3 rOffset = Random.insideUnitCircle;
                rOffset = (new Vector3(randomOffset.x, 0, randomOffset.y)).normalized * randomOffset.PickRandom();

                el.transform.localPosition += offset + rOffset;
                el.transform.rotation *= Quaternion.Euler(0, randomYRotationRange.PickRandom(), 0);
            }
        }
    }
}
