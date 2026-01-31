using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject gameOverUI;

    [Header("Game Data")]
    public float distance;
    public bool isPlaying;

    void Awake()
    {
        // ทำให้ GameManager เข้าถึงได้จากทุก Script
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        distance = 0f;
        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        // นับระยะทาง (ปรับเลข 5f ได้)
        distance += Time.deltaTime * 5f;
    }

    // ถูกเรียกเมื่อตาย
    public void GameOver()
    {
        Debug.Log("Game Over Triggered!");

        isPlaying = false;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    // ปุ่ม Restart
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ปุ่ม Menu
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuTest");
    }
}
