using UnityEngine;
using TMPro;

public class HUDUI : MonoBehaviour
{
    public TextMeshProUGUI distanceText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.distance;

        // ปรับแก้บรรทัดนี้เพื่อแสดงผลเป็น "เลข + M"
        distanceText.text = Mathf.FloorToInt(d).ToString() + " M";
    }
}