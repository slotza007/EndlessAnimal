using UnityEngine;
using System.Collections.Generic; // [เพิ่ม] จำเป็นสำหรับการใช้ Dictionary

public class playerfix : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 20f;
    public float extraGravity = 60f;
    public float roadLimitX = 5f;

    [Header("Game Mechanics")]
    public float maxRideTime = 5f; // ยังคงใช้ดีดตัวผู้เล่นออก (Bucking)

    [Header("Rodeo Animation")]
    public float bounceSpeed = 18f;
    public float bounceHeight = 0.15f;
    public float tiltAmount = 25f;

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer;
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Rideable01 debugStartingAnimal;

    [Header("Animation")]
    public Animator anim;

    // State Variables
    private bool isJumping = false;
    private Rideable01 currentAnimal;
    private Rideable01 targetAnimal;
    private Rigidbody rb;
    private float verticalVelocity = 0f;
    private float currentRideTimer = 0f;

    // [เพิ่ม] สมุดจดบันทึกจำนวนสัตว์ที่ขี่ในรอบนี้ (Key=ID, Value=จำนวน)
    private Dictionary<int, int> sessionRideCounts = new Dictionary<int, int>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // ระบบเสกสัตว์ตามที่เลือกมา (ถ้ามี Database)
        SpawnSelectedAnimal();
    }

    void SpawnSelectedAnimal()
    {
        Rideable01 animalToRide = null;

        if (AnimalDatabase.Instance != null)
        {
            int selectedIndex = PlayerPrefs.GetInt("SelectedAnimal", 0);
            if (selectedIndex < AnimalDatabase.Instance.animals.Length)
            {
                GameObject prefab = AnimalDatabase.Instance.animals[selectedIndex].modelPrefab;
                if (prefab != null)
                {
                    GameObject newAnimalObj = Instantiate(prefab, transform.position, Quaternion.identity);
                    animalToRide = newAnimalObj.GetComponent<Rideable01>();
                }
            }
        }
        else
        {
            if (debugStartingAnimal != null) animalToRide = debugStartingAnimal;
        }

        if (animalToRide != null)
        {
            MountAnimal(animalToRide);
        }
        else
        {
            JumpOff();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying) return;

        // กด Space เพื่อกระโดดเอง
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) JumpOff();

        // กด E เพื่อขี่ตัวใหม่ (ตอนลอยอยู่)
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null) MountAnimal(targetAnimal);

        // ตอนขี่อยู่: นับเวลาถอยหลังเพื่อดีดตัว
        if (!isJumping && currentAnimal != null)
        {
            currentRideTimer += Time.deltaTime;

            // เพิ่มความเด้งตอนใกล้หมดเวลาเตือนผู้เล่น
            bounceHeight = (currentRideTimer > maxRideTime * 0.7f) ? 0.3f : 0.15f;

            // ครบเวลา -> ดีดตัวออกอัตโนมัติ (สัตว์พยศ)
            if (currentRideTimer >= maxRideTime) JumpOff();
        }
        HandleIndicator();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying) { rb.linearVelocity = Vector3.zero; return; }

        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            verticalVelocity -= extraGravity * Time.fixedDeltaTime;
            Vector3 targetVel = new Vector3(horizontal * strafeSpeed, verticalVelocity, forwardSpeed);
            rb.linearVelocity = targetVel;

            Vector3 currentPos = transform.position;
            currentPos.x = Mathf.Clamp(currentPos.x, -roadLimitX, roadLimitX);
            transform.position = currentPos;

            FindTargetAnimal();
        }
        else if (currentAnimal != null)
        {
            Vector3 move = new Vector3(horizontal * strafeSpeed, 0, forwardSpeed) * Time.fixedDeltaTime;
            currentAnimal.transform.Translate(move);

            Vector3 animalPos = currentAnimal.transform.position;
            animalPos.x = Mathf.Clamp(animalPos.x, -roadLimitX, roadLimitX);
            currentAnimal.transform.position = animalPos;
        }
    }

    void LateUpdate()
    {
        if (!isJumping && currentAnimal != null)
        {
            float bounceY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            Vector3 finalPosition = currentAnimal.mountPoint.position + new Vector3(0, bounceY, 0);
            transform.position = finalPosition;

            float horizontal = Input.GetAxis("Horizontal");
            Quaternion tiltRotation = Quaternion.Euler(0, 0, -horizontal * tiltAmount);
            transform.rotation = currentAnimal.mountPoint.rotation * tiltRotation;
        }
    }

    void JumpOff()
    {
        isJumping = true;
        if (currentAnimal != null) currentAnimal.SetRidden(false);
        currentAnimal = null;
        targetAnimal = null;
        if (anim != null) anim.SetBool("isJumping", true);
        rb.isKinematic = false;
        verticalVelocity = jumpPower;
    }

    void MountAnimal(Rideable01 newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;
        currentRideTimer = 0f;
        bounceHeight = 0.15f;
        newAnimal.SetRidden(true);

        if (anim != null) anim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = newAnimal.mountPoint.position;
        transform.rotation = newAnimal.mountPoint.rotation;

        // --- [แก้ไข] ระบบนับจำนวนเพื่อปลดล็อก ---
        if (AnimalDatabase.Instance != null)
        {
            int id = newAnimal.animalID;

            // 1. เพิ่มจำนวนการขี่
            if (!sessionRideCounts.ContainsKey(id))
            {
                sessionRideCounts[id] = 0;
            }
            sessionRideCounts[id]++;

            Debug.Log($"ขี่ตัว ID {id} ไปแล้ว {sessionRideCounts[id]} ตัว");

            // 2. เช็คว่าครบกำหนดหรือยัง
            // ตรวจสอบ Index เพื่อป้องกัน Error กรณี ID เกิน Database
            if (id >= 0 && id < AnimalDatabase.Instance.animals.Length)
            {
                int required = AnimalDatabase.Instance.animals[id].requiredAmount;

                // ถ้าขี่ครบตามจำนวนที่กำหนด ให้ปลดล็อก
                if (sessionRideCounts[id] >= required)
                {
                    AnimalDatabase.Instance.UnlockAnimal(id);
                }
            }
        }
    }

    void FindTargetAnimal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Rideable01 closest = null;
        float minDst = float.MaxValue;
        foreach (var hit in hits)
        {
            Rideable01 r = hit.GetComponent<Rideable01>();
            if (r != null && hit.transform.position.z > transform.position.z)
            {
                float dst = Vector3.Distance(transform.position, hit.transform.position);
                if (dst < minDst) { minDst = dst; closest = r; }
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
            Vector3 pos = (targetAnimal != null) ? targetAnimal.transform.position : transform.position + Vector3.forward * 5f;
            targetIndicator.transform.position = new Vector3(pos.x, indicatorHeight, pos.z);
        }
        else targetIndicator.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isJumping) return;
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}