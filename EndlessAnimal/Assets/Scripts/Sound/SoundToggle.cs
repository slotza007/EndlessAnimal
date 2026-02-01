using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour
{
    [Header("UI")]
    public Image soundIcon;

    [Header("Icon Colors")]
    public Color soundOnColor = Color.white;
    public Color soundOffColor = Color.gray;

    private bool isMuted;

    void Start()
    {
        // โหลดค่าที่เคยบันทึกไว้
        // 0 = เปิดเสียง, 1 = ปิดเสียง
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplySound();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;

        // บันทึกค่า
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);

        ApplySound();
    }

    void ApplySound()
    {
        // เปิด / ปิดเสียงทั้งเกม
        AudioListener.volume = isMuted ? 0f : 1f;

        // เปลี่ยนสี icon
        if (soundIcon != null)
        {
            soundIcon.color = isMuted ? soundOffColor : soundOnColor;
        }
    }
}
