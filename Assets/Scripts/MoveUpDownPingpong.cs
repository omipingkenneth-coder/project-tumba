using UnityEngine;

public class MoveUpDownPingPong : MonoBehaviour
{
    [SerializeField] private float distance = 2f;
    [SerializeField] private float speed = 2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.PingPong(Time.time * speed, distance);
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
