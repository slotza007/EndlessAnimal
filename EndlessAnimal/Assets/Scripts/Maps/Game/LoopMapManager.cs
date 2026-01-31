using UnityEngine;
using System.Collections.Generic;

public class LoopMapManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Section Prefabs")]
    public List<GameObject> sectionPrefabs;

    [Header("Settings")]
    public float sectionLength = 40f;
    public int startSectionCount = 5;

    private float nextSpawnZ;
    private GameObject lastSpawnedPrefab;
    private Queue<GameObject> spawnedSections = new Queue<GameObject>();

    // 👉 ตัวแปรใหม่: ใช้เช็ค section แรก
    private bool isFirstSection = true;

    void Start()
    {
        nextSpawnZ = 0f;

        // สร้าง section เริ่มต้น
        for (int i = 0; i < startSectionCount; i++)
        {
            SpawnSection();
        }
    }

    void Update()
    {
        if (spawnedSections.Count == 0) return;

        GameObject firstSection = spawnedSections.Peek();

        if (player.position.z - firstSection.transform.position.z > sectionLength)
        {
            Destroy(spawnedSections.Dequeue());
            SpawnSection();
        }
    }

    void SpawnSection()
    {
        GameObject prefab;

        // 👉 section แรก fix เป็น Map01 (Element 0)
        if (isFirstSection)
        {
            prefab = sectionPrefabs[0];
            isFirstSection = false;
        }
        else
        {
            prefab = GetRandomPrefab();
        }

        GameObject section = Instantiate(
            prefab,
            new Vector3(0, 0, nextSpawnZ),
            Quaternion.identity,
            transform
        );

        spawnedSections.Enqueue(section);
        lastSpawnedPrefab = prefab;
        nextSpawnZ += sectionLength;
    }

    GameObject GetRandomPrefab()
    {
        if (sectionPrefabs.Count == 1)
            return sectionPrefabs[0];

        GameObject chosen;

        do
        {
            chosen = sectionPrefabs[Random.Range(0, sectionPrefabs.Count)];
        }
        while (chosen == lastSpawnedPrefab);

        return chosen;
    }
}
