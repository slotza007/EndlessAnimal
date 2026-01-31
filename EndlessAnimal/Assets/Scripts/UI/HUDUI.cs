using UnityEngine;
using TMPro;

public class HUDUI : MonoBehaviour
{
    public TextMeshProUGUI distanceText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.distance;
        distanceText.text = "Distance : " + Mathf.FloorToInt(d);
    }
}
