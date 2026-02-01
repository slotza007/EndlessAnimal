using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("3D Showcase")]
    public Transform modelHolder;
    public float rotateSpeed = 30f;

    [Header("UI Elements")]
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI animalNameText;
    public TextMeshProUGUI lockConditionText;
    public Button playButton;
    public TextMeshProUGUI playButtonText;

    private int currentIndex = 0;
    private GameObject currentModel;

    void Start()
    {
        // โชว์คะแนน
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (bestScoreText != null) bestScoreText.text = "BEST: " + bestScore + "m";

        // โหลดตัวเลือกสัตว์ล่าสุด
        currentIndex = PlayerPrefs.GetInt("SelectedAnimal", 0);
        UpdateAnimalDisplay();
    }

    void Update()
    {
        // หมุนโชว์ตัว
        if (modelHolder != null)
        {
            modelHolder.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }
    }

    public void NextAnimal()
    {
        currentIndex++;
        if (currentIndex >= AnimalDatabase.Instance.animals.Length) currentIndex = 0;
        UpdateAnimalDisplay();
    }

    public void PreviousAnimal()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = AnimalDatabase.Instance.animals.Length - 1;
        UpdateAnimalDisplay();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("MainsceneTest01");
    }

    void UpdateAnimalDisplay()
    {
        if (currentModel != null) Destroy(currentModel);

        AnimalData data = AnimalDatabase.Instance.animals[currentIndex];

        if (data.modelPrefab != null)
        {
            currentModel = Instantiate(data.modelPrefab, modelHolder);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            // ========================================================
            // [จุดแก้ไข] : ปิดสคริปต์ Rideable01 เพื่อไม่ให้วิ่งในหน้าเมนู
            // ========================================================

            // 1. ค้นหาสคริปต์ Rideable01 ในตัวที่เพิ่งสร้าง
            Rideable01 runScript = currentModel.GetComponent<Rideable01>();

            if (runScript != null)
            {
                // สั่งปิดสคริปต์ทันที -> ผลคือ Update() จะไม่ทำงาน มันจะยืนนิ่งๆ ไม่วิ่งไปข้างหน้า
                runScript.enabled = false;
            }

            // 2. ปิด Root Motion ของ Animator ด้วย (เผื่อท่าวิ่งมันดึงตัวไปข้างหน้า)
            Animator anim = currentModel.GetComponent<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
            }
            // ========================================================
        }

        if (animalNameText != null) animalNameText.text = data.animalName;

        if (data.isUnlocked)
        {
            playButton.interactable = true;
            playButtonText.text = "RUN!";
            if (lockConditionText != null) lockConditionText.text = "Ready to ride";
            PlayerPrefs.SetInt("SelectedAnimal", currentIndex);
            PlayerPrefs.Save();
        }
        else
        {
            playButton.interactable = false;
            playButtonText.text = "LOCKED";
            if (lockConditionText != null) lockConditionText.text = $"Ride for {data.tameDuration}s to get!";
        }
    }
}