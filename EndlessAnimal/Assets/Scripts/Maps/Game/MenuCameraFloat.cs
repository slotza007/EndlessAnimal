using UnityEngine;

public class MenuCameraFloat : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public float speed = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveAmount;
        transform.position = startPos + new Vector3(offset, 0f, 0f);
    }
}
