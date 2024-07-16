namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridFillLineBehavior : LineBehaviorSpawner
    {
        public float spacing = 2.5f;
        [Range(0, 360)] public float randomY = 0;
        [Range(0, 1)] public float spawnChance = 1;
        public Vector2 scaleRange = Vector2.one;
        public Vector2 randomOffset;

        public override List<SpawnPosition> UpdatePositions()
        {
            Random.InitState(0);

            var positions = line.GetPointsInsideGrid(spacing).ConvertAll(x => new SpawnPosition() { position = x + Random.insideUnitSphere.MaskY(0)*randomOffset.PickRandom(), scale = Vector3.one * scaleRange.PickRandom(), rotation = Quaternion.Euler(0, randomY * Random.value, 0) });
            positions.RemoveAll(x => Random.value >= spawnChance);
            return positions;
        }
    }
}
