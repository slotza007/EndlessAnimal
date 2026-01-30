using UnityEngine;

public class Rideable : MonoBehaviour
{
    public Transform mountPoint;
    public float moveSpeed = 10f;

    void OnTriggerEnter(Collider other)
    {
        // [เพิ่มบรรทัดนี้] เพื่อเช็กว่ามันชนอะไรบ้าง (ดูใน Console)
        Debug.Log("สัตว์วิ่งชนกับ: " + other.gameObject.name + " (Tag: " + other.tag + ")");

        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over!"); // เช็กว่าเข้าเงื่อนไขไหม

            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.GameOver();
            }
        }
    }
}