using UnityEngine;

public class Rideable01 : MonoBehaviour
{
    [Header("Identity")]
    // [สำคัญ] ต้องมีตัวแปรนี้ ไม่งั้น PlayerRodeoController จะ Error
    public int animalID;

    [Header("Setup")]
    public Transform mountPoint;

    [Header("Movement")]
    public float runSpeed = 5f;
    public float lifeTime = 15f;

    public bool isBeingRidden = false;

    void Start()
    {
        Invoke("DestroySelf", lifeTime);
    }

    void DestroySelf()
    {
        if (!isBeingRidden) Destroy(gameObject);
    }

    void Update()
    {
        if (!isBeingRidden)
        {
            transform.Translate(Vector3.forward * runSpeed * Time.deltaTime);
            if (transform.position.y < -5f) Destroy(gameObject);
        }
    }

    public void SetRidden(bool status)
    {
        isBeingRidden = status;
        if (status) CancelInvoke("DestroySelf");
    }

    void OnTriggerEnter(Collider other)
    {
        if (isBeingRidden && other.CompareTag("Obstacle"))
        {
            Debug.Log("Game Over: ชนกับ " + other.gameObject.name);
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
    }
}