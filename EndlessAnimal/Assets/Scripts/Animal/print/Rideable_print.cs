using UnityEngine;

public class Rideable_print : MonoBehaviour
{
    public Transform mountPoint; // จุดสำหรับให้คนนั่ง
    public float moveSpeed = 10f; // ความเร็วของสัตว์ตัวนี้

    private Animator anim;

    void Awake()
    {
        // ค้นหา Animator ในตัวสัตว์โดยอัตโนมัติ
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }
}