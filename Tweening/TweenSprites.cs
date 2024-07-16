namespace MarTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TweenSprites : TweenCore
    {
        public List<Sprite> Sprites;

        public override void SetPose(float t)
        {
            if (Sprites == null || Sprites.Count == 0) return;

            t = Mathf.Clamp01(t);

            Sprite sprite = Sprites[Mathf.FloorToInt(t*(Sprites.Count-1))];

            if(TryGetComponent<Image>(out Image img))
            {
                img.sprite = sprite;
            }
            else if(TryGetComponent<SpriteRenderer>(out SpriteRenderer spr))
            {
                spr.sprite = sprite;
            }
        }
    }
}

