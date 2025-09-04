using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneWithCamera : MonoBehaviour
{
    [Header("Config")]
    public KeyCode toggleKey = KeyCode.F;
    public GameObject phoneUI;
    public int maxBattery = 3;
    private int currentBattery;
    private bool isPhoneOpen = false;

    [Header("UI de batería")]
    public Image[] batteryIcons;
    public Sprite batteryOnSprite;
    public Sprite batteryOffSprite;

    void Start()
    {
        currentBattery = maxBattery;
        phoneUI.SetActive(false);
        UpdateBatteryUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePhone();
        }
    }

    void TogglePhone()
    {
        if (isPhoneOpen)
        {
            phoneUI.SetActive(false);
            isPhoneOpen = false;
        }
        else
        {
            if (currentBattery > 0)
            {
                phoneUI.SetActive(true);
                isPhoneOpen = true;
                currentBattery--;
                UpdateBatteryUI();
            }
            else
            {
                Debug.Log("Teléfono apagado, sin batería.");
            }
        }
    }

    void UpdateBatteryUI()
    {
        for (int i = 0; i < batteryIcons.Length; i++)
        {
            bool isOn = i < currentBattery;
            if (batteryIcons[i] != null)
            {
                if (batteryOnSprite && batteryOffSprite)
                    batteryIcons[i].sprite = isOn ? batteryOnSprite : batteryOffSprite;
                else
                    batteryIcons[i].enabled = isOn;
            }
        }
    }
}