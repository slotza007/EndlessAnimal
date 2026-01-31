using UnityEngine;

public class Rideable01 : MonoBehaviour
{
    [Header("Setup")]
    public Transform mountPoint; // จุดเกาะของผู้เล่น

    [Header("Movement")]
    public float runSpeed = 5f;      // ความเร็วตอนวิ่งหนี (ตอนไม่มีคนขี่)
    public float lifeTime = 15f;    // เวลาชีวิตก่อนหายไป

    // ตัวแปรเช็คว่ามีคนขี่อยู่ไหม
    public bool isBeingRidden = false;

    void Start()
    {
        // ตั้งเวลาทำลายตัวเอง (ถ้าไม่มีคนมาขี่)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // ถ้าไม่มีคนขี่ ให้วิ่งไปข้างหน้าเอง
        if (!isBeingRidden)
        {
            transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);

            // ถ้าตกเหว (ต่ำกว่า Y -5) ให้ลบทิ้ง
            if (transform.position.y < -5f) Destroy(gameObject);
        }
    }

    // ฟังก์ชันสั่งสลับโหมด (ขี่ / ปล่อย)
    public void SetRidden(bool status)
    {
        isBeingRidden = status;
        if (status)
        {
            CancelInvoke(); // ถ้าถูกขี่แล้ว ยกเลิกการนับเวลาตาย
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ถ้าขี่อยู่ และชนกับสิ่งกีดขวาง (ไม่ว่าจะชนมุมไหน) -> Game Over
        if (isBeingRidden && other.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over: ชนกับ " + other.gameObject.name);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}