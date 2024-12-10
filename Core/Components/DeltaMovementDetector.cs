using MarTools;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DeltaMovementDetector : MonoBehaviour
{
    public UnityEvent<float> OnUpdate;
    public Vector2 inputSpace = new Vector2(0, 5);
    public Vector2 outputSpace = new Vector2(0, 1);

    public float velocity { get; private set; }

    Vector3 lastPosition;
    float lastDelta;

    private void Awake()
    {
        lastDelta = Time.deltaTime;
        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        velocity = delta.magnitude / lastDelta;
        lastDelta = Time.deltaTime;

        float mapped = velocity.Remap(inputSpace, outputSpace);
        OnUpdate.Invoke(mapped);
    }
}
