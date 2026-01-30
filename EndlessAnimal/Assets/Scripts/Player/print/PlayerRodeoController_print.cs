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
    public LayerMask animalLayer; // ต้องตั้งเป็น Animal ใน Inspector
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Rideable_print startingAnimal;

    private bool isJumping = false;
    private Rideable_print currentAnimal;
    private Rideable_print targetAnimal;
    private Rigidbody rb;

    [Header("Animation")]
    public Animator playerAnim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (startingAnimal != null) MountAnimal(startingAnimal);
        else JumpOff();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) JumpOff();
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null) MountAnimal(targetAnimal);
        HandleIndicator();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (isJumping)
        {
            Vector3 targetVel = new Vector3(horizontal * strafeSpeed, rb.linearVelocity.y, forwardSpeed);
            rb.linearVelocity = targetVel;
            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
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
        if (playerAnim != null) playerAnim.SetBool("isJumping", true);
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * jumpPower + Vector3.forward * pushForwardForce, ForceMode.Impulse);
    }

    void MountAnimal(Rideable_print newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        if (playerAnim != null) playerAnim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    void FindTargetAnimal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Rideable_print closest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            Rideable_print rideable = hit.GetComponent<Rideable_print>();
            if (rideable != null && rideable != currentAnimal && hit.transform.position.z > transform.position.z)
            {
                float dst = Vector3.Distance(transform.position, hit.transform.position);
                if (dst < minDst) { minDst = dst; closest = rideable; }
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
            Vector3 targetPos = targetAnimal != null ? targetAnimal.transform.position : transform.position + Vector3.forward * 5f;
            targetIndicator.transform.position = new Vector3(targetPos.x, indicatorHeight, targetPos.z);
        }
        else targetIndicator.SetActive(false);
    }
}