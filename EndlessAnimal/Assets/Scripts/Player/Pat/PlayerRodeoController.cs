using UnityEngine;

public class PlayerRodeoController : MonoBehaviour
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
    public Rideable startingAnimal;

    // State
    private bool isJumping = false;
    private Rideable currentAnimal;
    private Rideable targetAnimal;
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
        // 🔴 กัน GameOver ซ้ำ
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
            return;

        // 🔴 กันตกหลุด map (สำคัญมาก)
        if (transform.position.y < -5f)
        {
            TriggerGameOver("Fell off map");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            JumpOff();

        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null)
            MountAnimal(targetAnimal);

        HandleIndicator();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            // --- โหมดลอยตัว ---
            verticalVelocity -= extraGravity * Time.fixedDeltaTime;

            Vector3 targetVel = new Vector3(
                horizontal * strafeSpeed,
                verticalVelocity,
                forwardSpeed
            );

            rb.linearVelocity = targetVel;

            // [เพิ่ม] ล็อคตำแหน่งคน ไม่ให้หลุดขอบตอนลอย
            Vector3 currentPos = transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, -roadLimitX, roadLimitX);
            transform.position = currentPos;

            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            // --- โหมดขี่สัตว์ ---
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;

            // สั่งขยับ
            currentAnimal.transform.Translate(move);

            // [เพิ่ม] ล็อคตำแหน่งสัตว์ ไม่ให้วิ่งหลุดขอบ
            Vector3 animalPos = currentAnimal.transform.position;

            // คำสั่ง Clamp จะล็อคค่าให้อยู่ระหว่าง min กับ max เสมอ
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
        isJumping = true;
        currentAnimal = null;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", true);

        rb.isKinematic = false;
        verticalVelocity = jumpPower;
    }

    void MountAnimal(Rideable newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;

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
            Rideable r = hit.GetComponent<Rideable>();
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

    void OnCollisionEnter(Collision collision)
    {
        if (!isJumping) return;

        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            TriggerGameOver("Hit " + collision.gameObject.name);
        }
    }

    // 🔥 ฟังก์ชันกลางจบเกม (สำคัญมาก)
    void TriggerGameOver(string reason)
    {
        Debug.Log("Game Over: " + reason);

        if (GameManager.Instance != null && GameManager.Instance.isPlaying)
        {
            GameManager.Instance.GameOver();

            GameOverUI ui = FindFirstObjectByType<GameOverUI>();
            if (ui != null)
            {
                ui.Show();
            }
            else
            {
                Debug.LogError("❌ GameOverUI not found in scene");
            }
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
}
