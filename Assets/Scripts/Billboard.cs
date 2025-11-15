using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        var cam = Camera.main.transform;
        var target = transform.position + (transform.position - cam.position);
        target.y = transform.position.y;

        transform.LookAt(target);

    }
}
