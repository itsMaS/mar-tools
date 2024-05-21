namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridFillLineBehavior : LineBehaviorSpawner
    {
        public float spacing = 2.5f;
        public override List<SpawnPosition> UpdatePositions()
        {
            Random.InitState(0);
            return line.GetPointsInsideGrid(spacing).ConvertAll(x => new SpawnPosition() { position = x, scale = Vector3.one * Random.Range(1,5), rotation = Quaternion.Euler(0,360*Random.value,0)});
        }
    }
}
