using UnityEngine;
using TMPro; // จำเป็นสำหรับการใช้ TextMeshPro

public class GameOverUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panel;
    public TextMeshProUGUI distanceText; // คะแนนรอบนี้
    public TextMeshProUGUI bestScoreText; // [เพิ่ม] คะแนนสูงสุด

    [Header("External UI")]
    public GameObject hudPanel; // หน้าจอ HUD ตอนเล่น (เอาไว้สั่งปิด)

    void Start()
    {
        panel.SetActive(false); // ซ่อนตัวเองตอนเริ่มเกม

        // รอฟัง Event จาก GameManager ว่าจบเกมหรือยัง
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += Show;
        }
    }

    void OnDestroy()
    {
        // เลิกฟังเมื่อ object ถูกทำลาย (กัน error)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= Show;
        }
    }

    // ฟังก์ชันนี้ทำงานเมื่อจบเกม
    public void Show()
    {
        // 1. เปิดหน้าจอ Game Over
        panel.SetActive(true);

        // 2. ปิดหน้าจอ HUD (ระยะทางวิ่ง)
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        // 3. ระบบคำนวณคะแนน
        if (GameManager.Instance != null)
        {
            // ดึงระยะทางวิ่งมาจาก GameManager (ปัดเศษทิ้ง)
            int currentScore = Mathf.FloorToInt(GameManager.Instance.distance);

            // ดึงคะแนนสูงสุดที่เคยทำได้ (ถ้าไม่มีให้เป็น 0)
            int oldBestScore = PlayerPrefs.GetInt("BestScore", 0);

            // ตรวจสอบว่าทำลายสถิติไหม?
            if (currentScore > oldBestScore)
            {
                // บันทึกสถิติใหม่ลงเครื่อง
                PlayerPrefs.SetInt("BestScore", currentScore);
                PlayerPrefs.Save(); // สั่ง save ทันที

                // อัปเดตตัวแปรเพื่อโชว์ค่าใหม่
                oldBestScore = currentScore;

                // (Optional) อาจจะใส่ Effect ข้อความ "NEW RECORD!" ตรงนี้
            }

            // 4. แสดงผลบนหน้าจอ
            distanceText.text = "Distance: " + currentScore + "m";

            if (bestScoreText != null)
            {
                bestScoreText.text = "Best: " + oldBestScore + "m";
            }
        }
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    public void OnMenuButton()
    {
        if (GameManager.Instance != null) GameManager.Instance.GoToMenu();
    }
}