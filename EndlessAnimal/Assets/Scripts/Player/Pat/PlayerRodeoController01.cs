using UnityEngine;
using System.Collections;
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
    public float mountDuration = 0.15f;

    [Header("Target System (Cone Detection)")]
    public float searchRadius = 8f;
    [Range(0, 360)] public float viewAngle = 90f; // [เพิ่ม] องศาการมองเห็น (กรวย)
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
    private bool isMounting = false;
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
            MountAnimalImmediate(animalToRide);
        else
            JumpOff();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
        {
            StopRunningSound();
            return;
        }

        if (!isJumping && currentAnimal != null && !isMounting)
        {
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
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == runningSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !isMounting) JumpOff();

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
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
        if (audioSource != null) audioSource.clip = null;

        isJumping = true;
        isMounting = false;

        if (currentAnimal != null) currentAnimal.SetRidden(false);
        currentAnimal = null;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", true);

        rb.isKinematic = false;
        verticalVelocity = jumpPower;
    }

    IEnumerator SmoothMount(Rideable01 newAnimal)
    {
        isJumping = false;
        isMounting = true;

        currentAnimal = newAnimal;
        targetAnimal = null;

        if (anim != null) anim.SetBool("isJumping", false);
        if (targetIndicator != null) targetIndicator.SetActive(false);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        newAnimal.SetRidden(true);

        float timer = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (timer < mountDuration)
        {
            timer += Time.deltaTime;
            float t = timer / mountDuration;

            transform.position = Vector3.Lerp(startPos, newAnimal.mountPoint.position, t);
            transform.rotation = Quaternion.Lerp(startRot, newAnimal.mountPoint.rotation, t);

            yield return null;
        }

        isMounting = false;
        MountSetup(newAnimal);
    }

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

    void MountSetup(Rideable01 newAnimal)
    {
        if (audioSource != null) audioSource.Stop();

        currentRideTimer = 0f;
        bounceHeight = 0.15f;

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

    // --- [แก้ไขใหม่] ระบบค้นหาเป้าหมายแบบกรวย (Cone) ---
    void FindTargetAnimal()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, animalLayer);
        Rideable01 closest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            Rideable01 r = hit.GetComponent<Rideable01>();

            // ตรวจสอบว่ามี Script Rideable01 ไหม
            if (r != null)
            {
                // 1. หาทิศทางไปยังสัตว์ตัวนั้น
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

                // 2. คำนวณมุมองศาระหว่าง "ทิศหน้าตรง (World Forward)" กับ "ทิศทางสัตว์"
                // ใช้ Vector3.forward แทน transform.forward เพื่อให้กรวยพุ่งตรงไปตามถนนเสมอ ไม่เอียงตามตัวละคร
                if (Vector3.Angle(Vector3.forward, dirToTarget) < viewAngle / 2f)
                {
                    // 3. ถ้าอยู่ในมุมมองแล้ว ค่อยเช็คระยะ
                    float dst = Vector3.Distance(transform.position, hit.transform.position);
                    if (dst < minDst)
                    {
                        minDst = dst;
                        closest = r;
                    }
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

    // --- [เพิ่ม] วาดเส้น Gizmos เพื่อให้เห็นพื้นที่กรวยในหน้า Scene ---
    void OnDrawGizmos()
    {
        // วาดวงกลมระยะค้นหา
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        // วาดเส้นขอบเขตกรวย (ซ้าย-ขวา)
        Vector3 leftDir = Quaternion.Euler(0, -viewAngle / 2f, 0) * Vector3.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle / 2f, 0) * Vector3.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * searchRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * searchRadius);
    }
}