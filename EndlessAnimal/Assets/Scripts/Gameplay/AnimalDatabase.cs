using UnityEngine;

[System.Serializable] // บรรทัดนี้สำคัญมาก ถ้าไม่มี Inspector จะมองไม่เห็น
public class AnimalData
{
    public string animalName;
    public GameObject modelPrefab;
    public bool isUnlocked;

    [Header("Unlock Settings")]
    // ตัวแปรนี้คือตัวที่จะไปโผล่ใน Inspector ครับ
    public int requiredAmount;
}

public class AnimalDatabase : MonoBehaviour
{
    public static AnimalDatabase Instance;
    public AnimalData[] animals;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        LoadSaveData();
    }

    void LoadSaveData()
    {
        for (int i = 0; i < animals.Length; i++)
        {
            if (i == 0) animals[i].isUnlocked = true;
            else
            {
                bool unlocked = PlayerPrefs.GetInt("Animal_" + i + "_Unlocked", 0) == 1;
                animals[i].isUnlocked = unlocked;
            }
        }
    }

    public void UnlockAnimal(int index)
    {
        if (index < animals.Length && !animals[index].isUnlocked)
        {
            animals[index].isUnlocked = true;
            PlayerPrefs.SetInt("Animal_" + index + "_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("🎉 CAUGHT NEW ANIMAL: " + animals[index].animalName);
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