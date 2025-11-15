using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Vertical offset for looking target")]
    public float heightOffset = 2f;

    void Update()
    {
        if (Camera.main == null) return;

        Transform cam = Camera.main.transform;

        // Calculate a target point in front of the camera
        Vector3 target = transform.position + (transform.position - cam.position);

        // Apply custom height
        target.y = transform.position.y + heightOffset;

        // Make this object look at the target
        transform.LookAt(target);
    }
}
