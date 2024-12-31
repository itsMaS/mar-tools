namespace MarTools
{
    using UnityEngine;

    public class FollowTransformSO : MonoBehaviour
    {
        public TransformSO target;

        private void Update()
        {
            if(target && target.Value)
            {
                transform.position = target.Value.position;
            }
        }
    }
}
