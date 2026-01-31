using UnityEngine;

public class PlayerRodeoController01 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float strafeSpeed = 8f;
    public float jumpPower = 20f;
    public float extraGravity = 60f;
    public float roadLimitX = 5f;

    [Header("Game Mechanics")]
    public float maxRideTime = 5f;

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
    // ไม่บังคับต้องใส่ใน Inspector แล้ว เพราะเราจะโหลดจาก Database
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // --- ส่วนที่แก้ไข: ระบบเสกสัตว์ตามที่เลือกมา ---
        SpawnSelectedAnimal();
    }

    void SpawnSelectedAnimal()
    {
        Rideable01 animalToRide = null;

        // 1. ตรวจสอบว่ามี AnimalDatabase อยู่ไหม (ต้องข้ามมาจากหน้า Menu)
        if (AnimalDatabase.Instance != null)
        {
            // ดึง index ที่เลือกไว้ (default คือ 0)
            int selectedIndex = PlayerPrefs.GetInt("SelectedAnimal", 0);

            // ดึง Prefab จาก Database
            GameObject prefab = AnimalDatabase.Instance.animals[selectedIndex].modelPrefab;

            // สร้างสัตว์ขึ้นมาที่ตำแหน่งเดียวกับผู้เล่น
            GameObject newAnimalObj = Instantiate(prefab, transform.position, Quaternion.identity);

            // ดึง Component Rideable01 ออกมา
            animalToRide = newAnimalObj.GetComponent<Rideable01>();
        }
        else
        {
            // ถ้าไม่มี Database (เช่น เทส Scene เกมเพียวๆ) ให้ใช้ตัว Debug ใน Inspector
            if (debugStartingAnimal != null)
            {
                animalToRide = debugStartingAnimal;
            }
        }

        // 2. ถ้ามีสัตว์ ให้ขี่เลย
        if (animalToRide != null)
        {
            MountAnimal(animalToRide);
        }
        else
        {
            // ถ้าไม่มีอะไรเลย ให้กระโดด
            JumpOff();
        }
    }

    // ... (ส่วน Update, FixedUpdate, LateUpdate, JumpOff, FindTargetAnimal, HandleIndicator คงเดิม) ...
    // ... (ก๊อปปี้ส่วนที่เหลือจากไฟล์เก่ามาใส่ตรงนี้ หรือใช้ไฟล์เก่าแล้วแก้แค่ Start กับเพิ่มฟังก์ชัน SpawnSelectedAnimal ก็ได้ครับ) ...

    // เพื่อความชัวร์ ผมใส่ Code ส่วนที่เหลือย่อๆ ไว้ให้นะครับ (Logic เดิมเป๊ะๆ)
    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) JumpOff();
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null) MountAnimal(targetAnimal);

        if (!isJumping && currentAnimal != null)
        {
            currentRideTimer += Time.deltaTime;
            bounceHeight = (currentRideTimer > maxRideTime * 0.7f) ? 0.3f : 0.15f;
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