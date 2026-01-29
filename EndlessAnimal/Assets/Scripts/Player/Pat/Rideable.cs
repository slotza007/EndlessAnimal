using UnityEngine;

public class Rideable : MonoBehaviour
{
    // จุดที่ผู้เล่นจะไปนั่ง (สร้าง Empty GameObject ชื่อ MountPoint ไว้ใน Cube สัตว์)
    public Transform mountPoint;

    // ไว้ปรับแต่งว่าสัตว์ตัวนี้วิ่งเร็วแค่ไหน (เดี๋ยว Role B มาทำต่อ)
    public float moveSpeed = 10f;
}