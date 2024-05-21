namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LineBehaviorScaleToEdge : LineBehaviorSpawner
    {
        public override List<SpawnPosition> UpdatePositions()
        {
            List<SpawnPosition> Positions = new List<SpawnPosition>();

            var points = line.smoothWorldPoints;
            for (int i = 0; i < points.Count - (line.looping ? 0 : 1); i++)
            {
                Vector3 last = points[i];
                Vector3 next = points[(i + 1) % points.Count];

                Vector3 dist = next - last;
                if (dist == Vector3.zero) continue;

                Quaternion rotation = Quaternion.LookRotation(dist, Vector3.up);
                Positions.Add(new SpawnPosition()
                {
                    position = Vector3.Lerp(last,next,0.5f),
                    scale = new Vector3(1,1, dist.magnitude),
                    rotation = rotation,
                });
            }
            return Positions;
        }
    }
}
