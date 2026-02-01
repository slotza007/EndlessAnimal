using UnityEngine;
using System.Collections; // จำเป็นสำหรับ IEnumerator
using System.Collections.Generic;

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

    // [เพิ่ม] ความเร็วในการพุ่งเข้าไปเกาะ (ยิ่งน้อยยิ่งเร็ว 0.1 - 0.2 กำลังดี)
    public float mountDuration = 0.15f;

    [Header("Target System")]
    public float searchRadius = 8f;
    public LayerMask animalLayer;
    public GameObject targetIndicator;
    public float indicatorHeight = 0.2f;

    [Header("Setup")]
    public Rideable01 debugStartingAnimal;

    [Header("Animation")]
    public Animator anim;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    public AudioClip runningSound;
    [Range(0f, 1f)] public float runningVolume = 0.5f;

    // State Variables
    private bool isJumping = false;
    private bool isMounting = false; // [เพิ่ม] เช็คว่ากำลังพุ่งไปเกาะอยู่ไหม
    private Rideable01 currentAnimal;
    private Rideable01 targetAnimal;
    private Rigidbody rb;
    private float verticalVelocity = 0f;
    private float currentRideTimer = 0f;

    private Dictionary<int, int> sessionRideCounts = new Dictionary<int, int>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (audioSource != null) audioSource.playOnAwake = false;

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
        else if (debugStartingAnimal != null)
        {
            animalToRide = debugStartingAnimal;
        }

        if (animalToRide != null)
        {
            // ตัวแรกให้วาร์ปไปเลย (ไม่ต้องสมูท)
            MountAnimalImmediate(animalToRide);
        }
        else
        {
            JumpOff();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
        {
            StopRunningSound();
            return;
        }

        // [แก้] เพิ่ม !isMounting เพื่อไม่ให้ทำงานตอนกำลังพุ่ง
        if (!isJumping && currentAnimal != null && !isMounting)
        {
            // --- Audio Logic ---
            if (audioSource != null && !audioSource.isPlaying && runningSound != null)
            {
                audioSource.clip = runningSound;
                audioSource.loop = true;
                audioSource.volume = runningVolume;
                audioSource.Play();
            }

            currentRideTimer += Time.deltaTime;
            bounceHeight = (currentRideTimer > maxRideTime * 0.7f) ? 0.3f : 0.15f;

            if (currentRideTimer >= maxRideTime) JumpOff();
        }
        else
        {
            // ถ้ากระโดด หรือกำลังพุ่งเกาะ ให้หยุดเสียงวิ่ง
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == runningSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        // ห้ามกระโดดซ้ำตอนกำลังพุ่งเกาะ (!isMounting)
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !isMounting) JumpOff();

        // กด E เพื่อพุ่งเกาะ (เรียก Coroutine แทนฟังก์ชันปกติ)
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null)
        {
            StartCoroutine(SmoothMount(targetAnimal));
        }

        HandleIndicator();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");

        if (isJumping)
        {
            verticalVelocity -= extraGravity * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(horizontal * strafeSpeed, verticalVelocity, forwardSpeed);

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
        // [แก้] เพิ่ม !isMounting เพื่อไม่ให้มันตีกับ Coroutine ที่กำลังเลื่อนตำแหน่ง
        if (!isJumping && currentAnimal != null && !isMounting)
        {
            float bounceY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.position = currentAnimal.mountPoint.position + new Vector3(0, bounceY, 0);

            float horizontal = Input.GetAxis("Horizontal");
            Quaternion tiltRotation = Quaternion.Euler(0, 0, -horizontal * tiltAmount);
            transform.rotation = currentAnimal.mountPoint.rotation * tiltRotation;
        }
    }

    void JumpOff()
    {
        // --- Audio Logic ---
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
        if (audioSource != null)
        {
            audioSource.clip = null; // เคลียร์คลิปเสียงวิ่ง
        }

        isJumping = true;
        isMounting = false; // เผื่อกดโดดตอนกำลังปีน (safety)

        if (currentAnimal != null) currentAnimal.SetRidden(false);
        currentAnimal = null;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", true);

        rb.isKinematic = false;
        verticalVelocity = jumpPower;
    }

    // --- [เพิ่มใหม่] ฟังก์ชันสำหรับทำให้การเกาะสมูท ---
    IEnumerator SmoothMount(Rideable01 newAnimal)
    {
        isJumping = false;
        isMounting = true; // บอกระบบว่ากำลังปีน (ห้ามขยับอย่างอื่น)

        currentAnimal = newAnimal;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        newAnimal.SetRidden(true); // สั่งสัตว์ว่าถูกขี่แล้ว

        // --- ช่วงเวลาแห่งการ Lerp (เคลื่อนที่สมูท) ---
        float timer = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (timer < mountDuration)
        {
            timer += Time.deltaTime;
            float t = timer / mountDuration;

            // ใช้ Lerp เพื่อค่อยๆ เคลื่อนไปหาจุดเกาะ
            transform.position = Vector3.Lerp(startPos, newAnimal.mountPoint.position, t);
            transform.rotation = Quaternion.Lerp(startRot, newAnimal.mountPoint.rotation, t);

            yield return null; // รอเฟรมถัดไป
        }

        // จบการปีน: เข้าสู่โหมดขี่ปกติ
        isMounting = false;
        MountSetup(newAnimal);
    }

    // ฟังก์ชันสำหรับตัวแรก (ไม่ต้องสมูท เพราะเริ่มมาก็ขี่เลย)
    void MountAnimalImmediate(Rideable01 newAnimal)
    {
        isJumping = false;
        currentAnimal = newAnimal;
        targetAnimal = null;
        newAnimal.SetRidden(true);

        if (anim != null) anim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = newAnimal.mountPoint.position;
        transform.rotation = newAnimal.mountPoint.rotation;

        MountSetup(newAnimal);
    }

    // ฟังก์ชันตั้งค่าต่างๆ (ใช้ร่วมกันทั้งแบบสมูทและแบบปกติ)
    void MountSetup(Rideable01 newAnimal)
    {
        if (audioSource != null) audioSource.Stop(); // หยุดเสียงเก่าก่อนเริ่มเสียงวิ่งใหม่

        currentRideTimer = 0f;
        bounceHeight = 0.15f;

        // --- Logic ปลดล็อกสัตว์ ---
        if (AnimalDatabase.Instance != null)
        {
            int id = newAnimal.animalID;
            if (!sessionRideCounts.ContainsKey(id)) sessionRideCounts[id] = 0;
            sessionRideCounts[id]++;

            if (id >= 0 && id < AnimalDatabase.Instance.animals.Length)
            {
                if (sessionRideCounts[id] >= AnimalDatabase.Instance.animals[id].requiredAmount)
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

    void StopRunningSound()
    {
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
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