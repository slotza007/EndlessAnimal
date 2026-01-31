using UnityEngine;

[System.Serializable]
public class AnimalData
{
    public string animalName;       // ชื่อสัตว์
    public GameObject modelPrefab;  // โมเดลสำหรับโชว์
    public bool isUnlocked;         // จับได้หรือยัง?

    [Header("Taming Settings")]
    public float tameDuration;      // ต้องขี่นานกี่วินาทีถึงจะจับได้ (เช่น 5 วินาที)
}

public class AnimalDatabase : MonoBehaviour
{
    public static AnimalDatabase Instance;
    public AnimalData[] animals;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadSaveData();
    }

    void LoadSaveData()
    {
        for (int i = 0; i < animals.Length; i++)
        {
            // ตัวแรก (Index 0) ให้ฟรีเสมอ
            if (i == 0)
            {
                animals[i].isUnlocked = true;
            }
            else
            {
                // เช็คจาก Save (1 = จับได้แล้ว)
                bool unlocked = PlayerPrefs.GetInt("Animal_" + i + "_Unlocked", 0) == 1;
                animals[i].isUnlocked = unlocked;
            }
        }
    }

    // ฟังก์ชันนี้เอาไว้ให้เพื่อน (Role Gameplay) เรียกใช้ตอนขี่ครบเวลา
    public void UnlockAnimal(int index)
    {
        // ถ้ายังไม่เคยจับได้ ให้ทำการปลดล็อก
        if (index < animals.Length && !animals[index].isUnlocked)
        {
            animals[index].isUnlocked = true;
            PlayerPrefs.SetInt("Animal_" + index + "_Unlocked", 1);
            PlayerPrefs.Save();

            Debug.Log("🎉 CAUGHT NEW ANIMAL: " + animals[index].animalName);

            // ตรงนี้อาจจะใส่ Code ให้ UI เด้งแจ้งเตือนว่า "จับสัตว์ใหม่ได้!" ในอนาคต
        }
    }

    public void SelectAnimal(int index)
    {
        if (animals[index].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedAnimal", index);
            PlayerPrefs.Save();
        }
    }

    public int GetSelectedAnimalIndex()
    {
        return PlayerPrefs.GetInt("SelectedAnimal", 0);
    }
}