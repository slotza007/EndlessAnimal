using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CollectionUI : MonoBehaviour
{
    [Header("3D Scene References")]
    public Transform modelHolder;       // จุดที่เอาโมเดลสัตว์มาวาง

    [Header("UI References")]
    public TextMeshProUGUI nameText;    // ชื่อสัตว์
    public TextMeshProUGUI infoText;    // ข้อความบอกเงื่อนไข (เช่น "Ride 10s to unlock")
    public Button selectButton;         // ปุ่มกดเลือก
    public TextMeshProUGUI selectButtonText; // ข้อความบนปุ่ม (SELECT / LOCKED)

    private int currentIndex = 0;
    private GameObject currentModel;

    void Start()
    {
        // เริ่มมาให้โชว์ตัวที่เลือกไว้ล่าสุด (ถ้าไม่มีก็ตัวแรก)
        currentIndex = AnimalDatabase.Instance.GetSelectedAnimalIndex();
        UpdateDisplay();
    }

    public void NextAnimal()
    {
        currentIndex++;
        if (currentIndex >= AnimalDatabase.Instance.animals.Length)
            currentIndex = 0; // วนกลับไปตัวแรก
        UpdateDisplay();
    }

    public void PreviousAnimal()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = AnimalDatabase.Instance.animals.Length - 1; // วนไปตัวท้าย
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        // 1. ลบโมเดลตัวเก่าทิ้ง
        if (currentModel != null) Destroy(currentModel);

        // 2. ดึงข้อมูลสัตว์ตัวปัจจุบัน
        AnimalData data = AnimalDatabase.Instance.animals[currentIndex];

        // 3. สร้างโมเดลตัวใหม่มาโชว์
        if (data.modelPrefab != null)
        {
            currentModel = Instantiate(data.modelPrefab, modelHolder);

            // ปรับตำแหน่งให้ตรง (Reset Transform)
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.Euler(0, 180, 0); // หันหน้าหาคนดู
            currentModel.transform.localScale = Vector3.one;
        }

        // 4. อัปเดตข้อความ UI
        nameText.text = data.animalName;

        // 5. เช็คสถานะล็อค/ปลดล็อค
        if (data.isUnlocked)
        {
            // ถ้าเลือกตัวนี้อยู่แล้ว
            if (currentIndex == AnimalDatabase.Instance.GetSelectedAnimalIndex())
            {
                selectButton.interactable = false;
                selectButtonText.text = "SELECTED";
                infoText.text = "Ready to ride!";
                infoText.color = Color.green;
            }
            else
            {
                selectButton.interactable = true;
                selectButtonText.text = "SELECT";
                infoText.text = "Unlocked";
                infoText.color = Color.white;
            }
        }
        else
        {
            // ยังล็อคอยู่
            selectButton.interactable = false;
            selectButtonText.text = "LOCKED";
            // ดึงค่า tameDuration มาโชว์
            infoText.text = $"Ride for {data.tameDuration}s to unlock";
            infoText.color = Color.red;
        }
    }

    // ฟังก์ชันกดปุ่ม Select
    public void OnSelectButton()
    {
        AnimalDatabase.Instance.SelectAnimal(currentIndex);
        UpdateDisplay(); // รีเฟรชหน้าจอเพื่อให้ปุ่มเปลี่ยนเป็น SELECTED
    }

    // ฟังก์ชันปุ่ม Back
    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenuTest"); // กลับหน้าเมนู
    }
}