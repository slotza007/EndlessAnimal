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

    // Event ให้ระบบอื่น (UI / Effect) มา subscribe
    public event Action OnGameOver;

    // =======================
    // 🔊 AUDIO (คุณดูแล)
    // =======================
    [Header("Audio")]
    public AudioSource bgmSource;     // เพลงหลัก
    public AudioSource gameOverSFX;   // เสียงแพ้

    private void Awake()
    {
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
            // คำนวณระยะทาง
            distance += Time.deltaTime * 10f;
        }
    }

    // =======================
    // 🟥 GAME OVER
    // =======================
    public void GameOver()
    {
        if (!isPlaying) return;

        isPlaying = false;

        // ⏸ หยุดเกม
        Time.timeScale = 0f;

        // 🔇 หยุดเพลงหลัก
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        // 🔊 เล่นเสียงแพ้
        if (gameOverSFX != null)
        {
            gameOverSFX.Play();
        }

        Debug.Log("🟥 GameManager: Game Over");

        // 📣 แจ้งระบบอื่น (UI, Effect)
        OnGameOver?.Invoke();
    }

    // =======================
    // 🔄 RESTART
    // =======================
    public void RestartGame()
    {
        Time.timeScale = 1f; // สำคัญมาก
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =======================
    // 🏠 BACK TO MENU
    // =======================
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuTest"); // เช็คชื่อ Scene ให้ตรง
    }
}
