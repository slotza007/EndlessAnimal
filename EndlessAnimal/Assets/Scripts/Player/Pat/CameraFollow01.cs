using UnityEngine;

public class CameraFollow01 : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f; // ค่าความหน่วง (ยิ่งน้อยยิ่งสมูท)
    public Vector3 offset; // ระยะห่างจากตัวละคร (ตั้งใน Inspector)

    void LateUpdate()
    {
        if (target == null) return;

        // คำนวณตำแหน่งที่กล้องควรจะอยู่
        Vector3 desiredPosition = target.position + offset;

        // ค่อยๆ เลื่อนกล้องไปหาตำแหน่งนั้น (Smooth)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // ล็อคแกน X (ถ้าไม่อยากให้กล้องส่ายซ้ายขวาตามตัวละครมากเกินไป)
        // smoothedPosition.x = transform.position.x; // ปลดคอมเมนต์บรรทัดนี้ถ้าอยากให้กล้องอยู่นิ่งๆ ตรงกลางถนน

        transform.position = smoothedPosition;
    }
}