using UnityEngine;

public class AnimalRunForward : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 10f; // ตั้งเวลาว่ากี่วินาทีจะให้ลบ (ปรับเปลี่ยนได้ใน Inspector)

    void Start()
    {
        // สั่งทำลายวัตถุนี้หลังจากเวลาผ่านไป lifeTime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}