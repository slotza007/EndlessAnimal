using UnityEngine;

public class PlayerRodeoController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 20f; // ปรับค่านี้แทนน้าหนักการกระโดด
    public float extraGravity = 60f; // แรงดึงดูดทำมือ (ยิ่งเยอะ ยิ่งลงไว)

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer;
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Rideable startingAnimal;

    // State
    private bool isJumping = false;
    private Rideable currentAnimal;
    private Rideable targetAnimal;
    private Rigidbody rb;

    // [ตัวแปรใหม่] เก็บความเร็วแนวดิ่งที่เราคำนวณเอง
    private float verticalVelocity = 0f;

    [Header("Animation")]
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ปิด Gravity ของ Unity ไปเลย เพราะเราจะคำนวณเอง
        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (startingAnimal != null)
        {
            MountAnimal(startingAnimal);
        }
        else
        {
            JumpOff();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            JumpOff();
        }

        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null)
        {
            MountAnimal(targetAnimal);
        }

        HandleIndicator();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            // --- โหมดลอยตัว (Manual Gravity) ---

            // 1. คำนวณแรงดึงดูด (ลดค่า Y ลงเรื่อยๆ ตามเวลา)
            verticalVelocity -= extraGravity * Time.fixedDeltaTime;

            // 2. เอาค่า Y ที่คำนวณได้ ไปใส่ในความเร็วรวมเลย (ไม่ต้องใช้ AddForce)
            Vector3 targetVel = new Vector3(
                horizontal * strafeSpeed,
                verticalVelocity, // ใช้ค่าที่เราคุมเอง
                forwardSpeed
            );

            rb.linearVelocity = targetVel;

            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            // --- โหมดขี่สัตว์ ---
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;
            currentAnimal.transform.Translate(move);
        }
    }

    void LateUpdate()
    {
        if (!isJumping && currentAnimal != null)
        {
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

        rb.isKinematic = false;

        // [จุดที่แก้] กำหนดความเร็วแกน Y ทันที (ดีดตัวขึ้น)
        verticalVelocity = jumpPower;

        // (Forward Force ไม่จำเป็นต้องใส่ตรงนี้ เพราะ FixedUpdate จะคุมความเร็วไปข้างหน้าให้อัตโนมัติแล้ว)
    }

    void MountAnimal(Rideable newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;

        // Reset ความเร็วตกเมื่อเกาะได้
        verticalVelocity = 0f;

        if (anim != null) anim.SetBool("isJumping", false);

        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

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
                targetIndicator.transform.position = new Vector3(
                    targetAnimal.transform.position.x,
                    indicatorHeight,
                    targetAnimal.transform.position.z
                );
            }
            else
            {
                targetIndicator.transform.position = new Vector3(
                    transform.position.x,
                    indicatorHeight,
                    transform.position.z + 5f
                );
            }
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }

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

    // ฟังก์ชันนี้จะทำงานทันทีที่ตัวคน (ที่มี CapsuleCollider) ไปชนกับอะไรสักอย่างที่มี Collider
    void OnCollisionEnter(Collision collision)
    {
        // เช็คว่าสิ่งที่ชนมี Tag ชื่อ "Ground" หรือไม่
        // (แถม: เช็ค Obstacle เผื่อไว้ด้วย กรณีโดดไปชนต้นไม้กลางอากาศ)
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over! ผู้เล่นตกพื้น/ชน: " + collision.gameObject.name);

            // เรียก GameManager ให้จบเกม
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.GameOver();
            }
        }
    }
}