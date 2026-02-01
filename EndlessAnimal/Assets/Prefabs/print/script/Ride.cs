using UnityEngine;

public class Ride : MonoBehaviour
{
    [Header("Setup")]
    public Transform mountPoint; // จุดที่ผู้เล่นจะไปนั่ง

    [Header("Movement")]
    public float runSpeed = 5f;      // ความเร็วตอนเป็นสัตว์ป่า
    public float lifeTime = 15f;     // เวลาที่สัตว์จะอยู่ในฉากก่อนถูกลบ

    private bool isBeingRidden = false;

    void Start()
    {
        // [แก้] เปลี่ยนจาก Destroy(gameObject, time) เป็น Invoke
        // เพื่อให้เราสามารถยกเลิกคำสั่งตายได้ตอนที่มีคนมาขี่
        Invoke("DestroySelf", lifeTime);
    }

    // ฟังก์ชันใหม่สำหรับสั่งทำลายตัวเอง
    void DestroySelf()
    {
        // เช็คอีกรอบเพื่อความชัวร์ ถ้าไม่มีคนขี่ค่อยลบ
        if (!isBeingRidden)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // ถ้าไม่มีคนขี่ ให้วิ่งไปข้างหน้าเอง
        if (!isBeingRidden)
        {
            // วิ่งตรงไปข้างหน้า (สวนทางผู้เล่น)
            transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);

            // หากตกแมพ (ต่ำกว่า Y -5) ให้ลบทิ้งทันที
            if (transform.position.y < -5f)
            {
                Destroy(gameObject);
            }
        }
    }

    // ฟังก์ชันสำหรับสลับโหมดควบคุม
    public void SetRidden(bool status)
    {
        isBeingRidden = status;

        if (status == true)
        {
            // [สำคัญ] ยกเลิกคำสั่ง DestroySelf ที่ตั้งไว้ใน Start
            // สัตว์จะอยู่ถาวรตราบใดที่เราขี่มัน
            CancelInvoke("DestroySelf");
        }
        else
        {
            // (Optional) ถ้ากระโดดลง อยากให้นับเวลาตายใหม่ก็เปิดบรรทัดล่างนี้
            // Invoke("DestroySelf", lifeTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบการชนสิ่งกีดขวาง (เฉพาะตอนขี่อยู่)
        if (other.CompareTag("Obstacle") && isBeingRidden)
        {
            // 1. หาความห่างระหว่างสัตว์กับวัตถุที่ชน
            Vector3 directionToObstacle = (other.transform.position - transform.position).normalized;

            // 2. เช็กทิศทางว่าวัตถุอยู่ด้านหน้าสัตว์หรือไม่ (ใช้ค่า Dot Product)
            // ค่า > 0.5f หมายถึงทำมุมประมาณ 60 องศาจากด้านหน้า
            float dot = Vector3.Dot(transform.forward, directionToObstacle);

            if (dot > 0.5f)
            {
                Debug.Log("Game Over: ชนด้านหน้ากับ " + other.gameObject.name);

                // ค้นหา GameManager แล้วสั่งจบเกม
                GameManager gm = Object.FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.GameOver();
                }
            }
            else
            {
                Debug.Log("รอด! เป็นการเบียดด้านข้างหรือชนจากทิศทางอื่น");
            }
        }
    }
}