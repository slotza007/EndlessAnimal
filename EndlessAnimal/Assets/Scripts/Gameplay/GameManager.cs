using UnityEngine;
using UnityEngine.SceneManagement;
using System; // จำเป็นต้องมีบรรทัดนี้เพื่อใช้ Action

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isPlaying = true;

    [Header("Game Data")]
    public float distance;

    // สร้าง Event ชื่อ OnGameOver (ลบตัวแปร gameOverUI ทิ้งไปได้เลย)
    public event Action OnGameOver;

    private void Awake()
    {
        // ตั้งค่า Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (isPlaying)
        {
            // เพิ่มระยะทางตามเวลา (ปรับตัวคูณได้ตามความเร็วเกม)
            distance += Time.deltaTime * 10f;
        }
    }

    // ฟังก์ชันจบเกม
    public void GameOver()
    {
        if (!isPlaying) return;

        isPlaying = false;
        Time.timeScale = 0f; // หยุดเวลา

        Debug.Log("🟥 GameManager: ประกาศ Event OnGameOver");

        // ตะโกนบอกทุกคนที่รอฟังว่า "จบเกมแล้ว!"
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลาให้เดินปกติ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuTest"); // ใส่ชื่อ Scene เมนูของคุณ
    }
}