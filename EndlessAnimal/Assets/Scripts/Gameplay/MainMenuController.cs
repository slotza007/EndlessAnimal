using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // ต้องใช้คุมปุ่ม

public class MainMenuController : MonoBehaviour
{
    [Header("3D Showcase")]
    public Transform modelHolder;       // จุดวางโมเดล (Empty Object หน้ากล้อง)
    public float rotateSpeed = 30f;     // ความเร็วหมุน

    [Header("UI Elements")]
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI animalNameText;
    public TextMeshProUGUI lockConditionText; // ข้อความเงื่อนไขปลดล็อก
    public Button playButton;           // ปุ่มเริ่มเกม
    public TextMeshProUGUI playButtonText; // ข้อความในปุ่มเริ่มเกม

    private int currentIndex = 0;
    private GameObject currentModel;

    void Start()
    {
        // 1. โชว์คะแนนสูงสุด (Code เดิม)
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (bestScoreText != null) bestScoreText.text = "BEST: " + bestScore + "m";

        // 2. เริ่มต้นให้โชว์ตัวที่เลือกไว้ล่าสุด
        currentIndex = PlayerPrefs.GetInt("SelectedAnimal", 0);
        UpdateAnimalDisplay();
    }

    void Update()
    {
        // สั่งให้โมเดลหมุนตลอดเวลา
        if (modelHolder != null)
        {
            modelHolder.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }
    }

    // --- ส่วนเลือกตัวละคร (Shop Logic) ---

    public void NextAnimal()
    {
        currentIndex++;
        if (currentIndex >= AnimalDatabase.Instance.animals.Length)
            currentIndex = 0;
        UpdateAnimalDisplay();
    }

    public void PrevAnimal()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = AnimalDatabase.Instance.animals.Length - 1;
        UpdateAnimalDisplay();
    }

    void UpdateAnimalDisplay()
    {
        // ลบโมเดลเก่า
        if (currentModel != null) Destroy(currentModel);

        // ดึงข้อมูลจาก Database
        AnimalData data = AnimalDatabase.Instance.animals[currentIndex];

        // สร้างโมเดลใหม่
        if (data.modelPrefab != null)
        {
            currentModel = Instantiate(data.modelPrefab, modelHolder);
            // ปรับตำแหน่งให้สวย (Reset Transform)
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }

        // อัปเดตชื่อ
        if (animalNameText != null) animalNameText.text = data.animalName;

        // --- จุดสำคัญ! เช็คว่าปลดล็อกหรือยัง ---
        if (data.isUnlocked)
        {
            // ถ้าปลดล็อกแล้ว: ให้กดเล่นได้
            playButton.interactable = true;
            playButtonText.text = "RUN!";
            if (lockConditionText != null) lockConditionText.text = "Ready to ride";

            // บันทึกตัวนี้เป็นตัวเลือกทันทีที่เลื่อนมาเจอ
            PlayerPrefs.SetInt("SelectedAnimal", currentIndex);
            PlayerPrefs.Save();
        }
        else
        {
            // ถ้ายังล็อก: ห้ามกดเล่น + บอกเงื่อนไข
            playButton.interactable = false;
            playButtonText.text = "LOCKED";
            if (lockConditionText != null)
                lockConditionText.text = $"Ride for {data.tameDuration}s to get!";
        }
    }

    // --- ปุ่ม Start (ทำงานเมื่อกด RUN) ---
    public void PlayGame()
    {
        SceneManager.LoadScene("MainsceneTest01");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}