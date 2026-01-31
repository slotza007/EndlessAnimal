using UnityEngine;
using UnityEngine.SceneManagement; // [สำคัญ] ต้องมีบรรทัดนี้ถึงจะเปลี่ยน Scene ได้

public class MainMenuController : MonoBehaviour
{
    // ฟังก์ชันนี้จะถูกเรียกเมื่อกดปุ่ม
    public void PlayGame()
    {
        // ใส่ชื่อ Scene เกมเพลย์ของคุณลงไปในนี้ (ต้องสะกดให้ตรงเป๊ะ 100%)
        // จากรูปภาพที่คุณส่งมา Scene เกมหลักชื่อ "MainsceneTest" ใช่ไหมครับ?
        SceneManager.LoadScene("MainsceneTest");
    }
}