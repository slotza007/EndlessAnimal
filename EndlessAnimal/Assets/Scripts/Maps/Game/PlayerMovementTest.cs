using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 6f;
    public float sideSpeed = 5f;
    public float laneLimit = 4f; // ขอบซ้าย-ขวาของทาง

    void Update()
    {
        // วิ่งไปข้างหน้าอัตโนมัติ
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // ซ้าย / ขวา
        float h = Input.GetAxis("Horizontal");
        Vector3 sideMove = Vector3.right * h * sideSpeed * Time.deltaTime;
        transform.Translate(sideMove);

        // จำกัดไม่ให้ออกนอกทาง
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -laneLimit, laneLimit);
        transform.position = pos;
    }
}
