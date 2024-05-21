namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EdgeFillLineBehavior : LineBehaviorSpawner
    {
        public float distanceBetweenPoints = 1f;
        public float sampleOffset = 0;
        public override List<SpawnPosition> UpdatePositions()
        {
            return line.GetPointsAlongLine(distanceBetweenPoints, sampleOffset).ConvertAll(x => new SpawnPosition() { position = x.Item1, rotation = Quaternion.LookRotation(x.Item2) });
        }
    }
} 
