using UnityEngine;

public class SimpleSway : MonoBehaviour
{
    public float angle = 5f;
    public float speed = 1f;

    void Update()
    {
        float sway = Mathf.Sin(Time.time * speed) * angle;
        transform.rotation = Quaternion.Euler(0, sway, 0);
    }
}
