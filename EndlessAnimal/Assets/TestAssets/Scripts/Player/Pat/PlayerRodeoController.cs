using UnityEngine;

public class PlayerRodeoController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 15f; // ความแรงในการดีดตัวขึ้นฟ้า
    public float pushForwardForce = 5f; // แรงส่งไปข้างหน้าตอนกระโดด

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer; // อย่าลืมตั้งเป็น Animal
    public GameObject targetIndicator; // ลาก Prefab วงกลมมาใส่
    public float indicatorHeight = 0.2f; // ความสูงของวงกลมจากพื้น

    [Header("Setup")]
    public Rideable startingAnimal;

    // State
    private bool isJumping = false;
    private Rideable currentAnimal;
    private Rideable targetAnimal;
    private Rigidbody rb;

    [Header("Animation")]
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Setup Rigidbody
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // ห้ามหมุน แต่ต้องขยับ Y ได้

        if (startingAnimal != null)
        {
            MountAnimal(startingAnimal);
        }
        else
        {
            JumpOff(); // ถ้าไม่มีสัตว์เริ่ม ให้ถือว่ากระโดดอยู่
        }
    }

    void Update()
    {
        // รับ Input กระโดดที่นี่ (เพื่อให้กดติดง่าย)
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            JumpOff();
        }

        // รับ Input ขี่สัตว์
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null)
        {
            MountAnimal(targetAnimal);
        }

        // อัปเดตตำแหน่งวงกลมเป้าหมาย
        HandleIndicator();
    }

    // ใช้ FixedUpdate คำนวณการเคลื่อนที่ (Physics)
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            // --- โหมดลอยตัว (Control กลางอากาศ) ---
            // เราจะใช้ Velocity บังคับ Rigidbody โดยตรง
            Vector3 currentVel = rb.linearVelocity;

            // คงความเร็วแนวดิ่ง (Y) ไว้ตามแรงโน้มถ่วง แต่เปลี่ยนแกน X และ Z
            Vector3 targetVel = new Vector3(
                horizontal * strafeSpeed,
                currentVel.y, // สำคัญ! ต้องใช้ค่าเดิมเพื่อให้ Gravity ทำงาน
                forwardSpeed
            );

            rb.linearVelocity = targetVel;

            // ค้นหาสัตว์ตลอดเวลาที่ลอย
            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            // --- โหมดขี่สัตว์ ---
            // สั่งให้สัตว์ขยับ (ใช้ Translate ได้เพราะสัตว์ไม่มี Rigidbody)
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;
            currentAnimal.transform.Translate(move);
        }
    }

    // ใช้ LateUpdate เพื่อล็อคตัวคนให้ติดสัตว์ (แก้ปัญหาตัวหลุด/สั่น)
    void LateUpdate()
    {
        if (!isJumping && currentAnimal != null)
        {
            // วาร์ปตัวคนไปที่ MountPoint ทันทีหลังจากสัตว์ขยับเสร็จแล้ว
            transform.position = currentAnimal.mountPoint.position;
            transform.rotation = currentAnimal.mountPoint.rotation;
        }
    }

    void JumpOff()
    {
        isJumping = true;
        currentAnimal = null;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", true);

        // --- จุดที่แก้ ---
        rb.isKinematic = false; // เปิดฟิสิกส์ก่อน
        rb.linearVelocity = Vector3.zero; // รีเซ็ตความเร็วด้วยคำสั่งใหม่
        rb.AddForce(Vector3.up * jumpPower + Vector3.forward * pushForwardForce, ForceMode.Impulse);
        // ----------------
    }

    void MountAnimal(Rideable newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;

        // [เพิ่ม] สั่งให้กลับมาเล่นท่านั่ง
        if (anim != null) anim.SetBool("isJumping", false);

        if (targetIndicator != null) targetIndicator.SetActive(false);

        // --- จุดที่แก้ ---
        // 1. หยุดความเร็วก่อน (ใช้คำสั่งใหม่ linearVelocity)
        rb.linearVelocity = Vector3.zero;

        // 2. แล้วค่อยเปิด Kinematic (ปิดฟิสิกส์)
        rb.isKinematic = true;
        // ----------------

        transform.position = newAnimal.mountPoint.position;
        transform.rotation = newAnimal.mountPoint.rotation;
    }

    void FindTargetAnimal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Rideable closest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            // ต้องเป็นสัตว์ตัวอื่น และอยู่ข้างหน้า
            if (hit.GetComponent<Rideable>() != currentAnimal && hit.transform.position.z > transform.position.z)
            {
                float dst = Vector3.Distance(transform.position, hit.transform.position);
                if (dst < minDst)
                {
                    minDst = dst;
                    closest = hit.GetComponent<Rideable>();
                }
            }
        }
        targetAnimal = closest;
    }

    void HandleIndicator()
    {
        if (targetIndicator == null) return;

        if (isJumping)
        {
            targetIndicator.SetActive(true);

            if (targetAnimal != null)
            {
                // ถ้าเจอสัตว์: วงกลมดูดไปที่สัตว์
                targetIndicator.transform.position = new Vector3(
                    targetAnimal.transform.position.x,
                    indicatorHeight,
                    targetAnimal.transform.position.z
                );
            }
            else
            {
                // ถ้าไม่เจอ: วงกลมลอยนำหน้าเรา
                targetIndicator.transform.position = new Vector3(
                    transform.position.x,
                    indicatorHeight,
                    transform.position.z + 5f // ระยะห่างวงกลม
                );
            }
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }

    // วาดเส้น Debug ในหน้า Scene View (เส้นแดงๆ)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        if (targetAnimal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetAnimal.transform.position);
        }
    }
}