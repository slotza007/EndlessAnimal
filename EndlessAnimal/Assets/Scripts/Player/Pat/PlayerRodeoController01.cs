using UnityEngine;
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
    [Range(0f, 1f)] public float jumpVolume = 1f;    // ปรับความดังเสียงกระโดดได้ที่นี่
    public AudioClip runningSound;
    [Range(0f, 1f)] public float runningVolume = 0.5f; // ปรับความดังเสียงวิ่งได้ที่นี่

    // State Variables
    private bool isJumping = false;
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

        if (animalToRide != null) MountAnimal(animalToRide);
        else JumpOff();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
        {
            StopRunningSound();
            return;
        }

        if (!isJumping && currentAnimal != null)
        {
            if (audioSource != null && !audioSource.isPlaying && runningSound != null)
            {
                audioSource.clip = runningSound;
                audioSource.loop = true;
                audioSource.volume = runningVolume; // ใช้ค่าจาก Inspector
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

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) JumpOff();
        if (isJumping && Input.GetKeyDown(KeyCode.E) && targetAnimal != null) MountAnimal(targetAnimal);

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
        if (!isJumping && currentAnimal != null)
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
            audioSource.PlayOneShot(jumpSound, jumpVolume); // ใช้ค่าความดังจาก Inspector
        }

        if (audioSource != null)
        {
            audioSource.clip = null;
        }

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
        if (audioSource != null) audioSource.Stop();

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