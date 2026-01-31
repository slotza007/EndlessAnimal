using UnityEngine;

public class Player12 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 20f;
    public float extraGravity = 60f;
    public float roadLimitX = 5f;

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer;
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Ride startingAnimal; // เปลี่ยนเป็น Ride

    // State
    private bool isJumping = false;
    private Ride currentAnimal; // เปลี่ยนเป็น Ride
    private Ride targetAnimal;  // เปลี่ยนเป็น Ride
    private Rigidbody rb;

    // manual gravity
    private float verticalVelocity = 0f;

    [Header("Animation")]
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (startingAnimal != null)
            MountAnimal(startingAnimal);
        else
            JumpOff();
    }

    void Update()
    {
        // กัน GameOver ซ้ำ
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
            return;

        // กันตกหลุด map
        if (transform.position.y < -5f)
        {
            TriggerGameOver("Fell off map");
            return;
        }

        // กด Space เพื่อกระโดดออก
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            JumpOff();

        // กด E เพื่อขี่สัตว์ตัวใหม่ขณะกำลังลอยตัว
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null)
            MountAnimal(targetAnimal);

        HandleIndicator();

        if (isJumping && transform.position.y < 0.01f)
        {
            Debug.Log("Game Over: ตัวละครตกพื้น");
            TriggerGameOver("ตกร่วงลงพื้น");
        }
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            // --- โหมดลอยตัวกลางอากาศ ---
            verticalVelocity -= extraGravity * Time.fixedDeltaTime;

            Vector3 targetVel = new Vector3(
                horizontal * strafeSpeed,
                verticalVelocity,
                forwardSpeed
            );

            rb.linearVelocity = targetVel;

            // ล็อคตำแหน่งคน ไม่ให้หลุดขอบตอนลอย
            Vector3 currentPos = transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, -roadLimitX, roadLimitX);
            transform.position = currentPos;

            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            // --- โหมดขี่สัตว์ ---
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;

            // สั่งสัตว์ตัวที่ขี่อยู่เคลื่อนที่
            currentAnimal.transform.Translate(move);

            // ล็อคตำแหน่งสัตว์ ไม่ให้วิ่งหลุดขอบถนน
            Vector3 animalPos = currentAnimal.transform.position;
            animalPos.x = Mathf.Clamp(animalPos.x, -roadLimitX, roadLimitX);
            currentAnimal.transform.position = animalPos;
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
        // สั่งให้สัตว์ตัวเก่ากลับไปวิ่งเอง
        if (currentAnimal != null)
        {
            // สั่งให้สัตว์หยุดสถานะการถูกขี่ (ถ้ามีโค้ดส่วนนี้อยู่)
            currentAnimal.SetRidden(false);

            // 🔥 เพิ่มบรรทัดนี้: ลบสัตว์ตัวที่เคยขี่ทิ้งทันที
            Destroy(currentAnimal.gameObject);
        }

        isJumping = true;
        currentAnimal = null;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", true);

        rb.isKinematic = false;
        verticalVelocity = jumpPower;
    }

    void MountAnimal(Ride newAnimal) // เปลี่ยน Parameter เป็น Ride
    {
        isJumping = false;
        currentAnimal = newAnimal;

        // 🔥 บรรทัดนี้สำคัญที่สุด: ต้องสั่งให้สัตว์ตัวใหม่ "เริ่ม" ตรวจจับการชน
        currentAnimal.SetRidden(true);

        targetAnimal = null;
        verticalVelocity = 0f;

        if (anim != null) anim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // ย้ายตำแหน่งเราไปที่จุดนั่งของสัตว์ตัวใหม่
        transform.position = newAnimal.mountPoint.position;
        transform.rotation = newAnimal.mountPoint.rotation;
    }

    void FindTargetAnimal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Ride closest = null; // เปลี่ยนเป็น Ride
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            Ride r = hit.GetComponent<Ride>(); // มองหา Script ชื่อ Ride
            if (r != null && r != currentAnimal && hit.transform.position.z > transform.position.z)
            {
                float dst = Vector3.Distance(transform.position, hit.transform.position);
                if (dst < minDst)
                {
                    minDst = dst;
                    closest = r;
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

            Vector3 pos = (targetAnimal != null)
                ? targetAnimal.transform.position
                : transform.position + Vector3.forward * 5f;

            targetIndicator.transform.position =
                new Vector3(pos.x, indicatorHeight, pos.z);
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }

    void TriggerGameOver(string reason)
    {
        if (GameManager.Instance != null && GameManager.Instance.isPlaying)
        {
            GameManager.Instance.GameOver();

            GameOverUI ui = Object.FindFirstObjectByType<GameOverUI>();
            if (ui != null)
            {
                ui.Show();
            }
        }
    }

}