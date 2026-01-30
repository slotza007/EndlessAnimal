using UnityEngine;

public class PlayerRodeoController_print : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 15f;
    public float pushForwardForce = 5f;

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer;
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Rideable_print startingAnimal;

    // State
    private bool isJumping = false;
    private Rideable_print currentAnimal;
    private Rideable_print targetAnimal;
    private Rigidbody rb;

    [Header("Animation")]
    public Animator playerAnim; // Animator ของตัวละครคน

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
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
        // กด Space เพื่อกระโดดออกจากสัตว์
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            JumpOff();
        }

        // กด E เพื่อขึ้นขี่สัตว์เป้าหมายในขณะลอยตัว
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
            // โหมดลอยตัว: ควบคุมทิศทางกลางอากาศ
            Vector3 currentVel = rb.linearVelocity;
            Vector3 targetVel = new Vector3(
                horizontal * strafeSpeed,
                currentVel.y,
                forwardSpeed
            );

            rb.linearVelocity = targetVel;
            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            // โหมดขี่สัตว์: เคลื่อนที่ไปข้างหน้า
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;
            currentAnimal.transform.Translate(move);
        }
    }

    void LateUpdate()
    {
        if (!isJumping && currentAnimal != null)
        {
            // ล็อคตัวละครให้ติดกับจุดขี่บนหลังสัตว์
            transform.position = currentAnimal.mountPoint.position;
            transform.rotation = currentAnimal.mountPoint.rotation;
        }
    }

    void JumpOff()
    {
        isJumping = true;
        currentAnimal = null;
        targetAnimal = null;

        if (playerAnim != null) playerAnim.SetBool("isJumping", true);

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * jumpPower + Vector3.forward * pushForwardForce, ForceMode.Impulse);
    }

    void MountAnimal(Rideable_print newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;

        if (playerAnim != null) playerAnim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = newAnimal.mountPoint.position;
        transform.rotation = newAnimal.mountPoint.rotation;
    }

    void FindTargetAnimal()
    {
        // ค้นหาสัตว์ในระยะรอบตัว
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Rideable_print closest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            Rideable_print rideable = hit.GetComponent<Rideable_print>();
            // ต้องเป็นสัตว์ตัวอื่นและอยู่ข้างหน้าผู้เล่นเท่านั้น
            if (rideable != null && rideable != currentAnimal && hit.transform.position.z > transform.position.z)
            {
                float dst = Vector3.Distance(transform.position, hit.transform.position);
                if (dst < minDst)
                {
                    minDst = dst;
                    closest = rideable;
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
                // เลื่อนวงกลมไปที่สัตว์เป้าหมาย
                targetIndicator.transform.position = new Vector3(targetAnimal.transform.position.x, indicatorHeight, targetAnimal.transform.position.z);
            }
            else
            {
                // เลื่อนวงกลมไปข้างหน้าผู้เล่น
                targetIndicator.transform.position = new Vector3(transform.position.x, indicatorHeight, transform.position.z + 5f);
            }
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }
}