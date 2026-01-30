using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการเปลี่ยนฉาก

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverUI; // ลาก GameOverPanel มาใส่ตรงนี้

    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นชน
    public void GameOver()
    {
        Debug.Log("Game Over Triggered!");

        // 1. เปิดหน้า UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 2. หยุดเวลา (หยุดทุกอย่างในเกม)
        Time.timeScale = 0f;
    }

    // ฟังก์ชันสำหรับปุ่ม Restart
    public void RestartGame()
    {
        // คืนค่าเวลากลับมาปกติก่อน (ไม่งั้นเริ่มเกมใหม่จะค้าง)
        Time.timeScale = 1f;

        // โหลด Scene ปัจจุบันใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ฟังก์ชันสำหรับปุ่ม Menu
    public void GoToMenu()
    {
        Time.timeScale = 1f;

        // ใส่ชื่อ Scene เมนูของคุณ (ต้องตรงเป๊ะ)
        SceneManager.LoadScene("MainMenuTest");
    }
}