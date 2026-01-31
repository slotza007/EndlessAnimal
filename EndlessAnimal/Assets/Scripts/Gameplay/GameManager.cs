using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isPlaying = true;

    [Header("Game Data")]
    public float distance;

    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    private void Update()
    {
        if (isPlaying)
        {
            distance += Time.deltaTime * 10f;
        }
    }

    public void GameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;
        Time.timeScale = 0f;
        Debug.Log("🟥 GameManager: Game Over");
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลาก่อนโหลดฉาก
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // [เพิ่ม] ฟังก์ชันกลับหน้าเมนู
    public void GoToMenu()
    {
        Time.timeScale = 1f; // คืนค่าเวลา
        SceneManager.LoadScene("MainMenuTest"); // **เช็คชื่อ Scene ให้ตรงกับไฟล์จริงของคุณ**
    }
}