using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animalPrefabs; // ลาก Prefab สัตว์มาใส่ใน Unity Inspector
    public float spawnRangeX = 10f;    // ระยะสุ่มแกน X
    public float spawnRangeZ = 10f;    // ระยะสุ่มแกน Z
    public int animalCount = 5;        // จำนวนที่จะให้เกิด

    void Start()
    {
        SpawnAnimals();
    }

    void SpawnAnimals()
    {
        for (int i = 0; i < animalCount; i++)
        {
            // 1. สุ่มเลือกสัตว์จาก Array
            int randomIndex = Random.Range(0, animalPrefabs.Length);

            // 2. สุ่มตำแหน่ง (อ้างอิงจากจุดที่วาง Script นี้)
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                0,
                Random.Range(-spawnRangeZ, spawnRangeZ)
            ) + transform.position;

            // 3. สร้างสัตว์ออกมา
            Instantiate(animalPrefabs[randomIndex], randomPos, Quaternion.identity);
        }
    }
}