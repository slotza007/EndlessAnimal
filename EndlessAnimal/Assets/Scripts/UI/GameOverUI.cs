using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panel;
    public TextMeshProUGUI distanceText;

    [Header("External UI")]
    public GameObject hudPanel; // เอาไว้ปิด HUD ตอนจบเกม

    void Start()
    {
        // ซ่อน Panel ตอนเริ่มเสมอ
        panel.SetActive(false);

        // วิ่งไปขอฟังข่าวจาก GameManager ว่า "ถ้าจบเกม ให้เรียกฟังก์ชัน Show นะ"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += Show;
        }
    }

    // สำคัญ: ต้องยกเลิกการฟังเมื่อหน้านี้ถูกทำลาย (กัน error)
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= Show;
        }
    }

    // ฟังก์ชันนี้จะทำงานเองเมื่อ GameManager ตะโกนบอก
    public void Show()
    {
        panel.SetActive(true);

        // ปิด HUD (ตัวบอกระยะทางตอนวิ่ง)
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        // แสดงคะแนน
        if (GameManager.Instance != null)
        {
            int score = Mathf.FloorToInt(GameManager.Instance.distance);
            distanceText.text = "Distance : " + score + "m";

            // (Optional) โค้ดส่วน Best Score ที่เคยคุยกันสามารถใส่เพิ่มตรงนี้ได้
        }
    }

    // ปุ่มกด Restart
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    // ปุ่มกดกลับเมนู
    public void OnMenuButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMenu();
        }
    }
}