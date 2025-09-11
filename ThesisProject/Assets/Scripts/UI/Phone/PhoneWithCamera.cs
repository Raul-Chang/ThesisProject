using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneWithCamera : MonoBehaviour
{
    [Header("Config")]
    public KeyCode toggleKey = KeyCode.F;
    public GameObject phoneUI;
    public int maxBattery = 4; 
    private int currentBattery;
    private bool isPhoneOpen = false;

    [Header("UI de batería")]
    public GameObject batteryFull;   
    public GameObject batteryHalf;   
    public GameObject batteryEmpty; 
    public GameObject Bateria; 
    public GameObject Bateria2; 

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
        
        batteryFull.SetActive(false);
        batteryHalf.SetActive(false);
        batteryEmpty.SetActive(false);
        Bateria.SetActive(false);
        Bateria2.SetActive(false);


        if (currentBattery == 4)
        {
            Bateria.SetActive(true);
            
        }
        else if (currentBattery == 3)
        {
            batteryFull.SetActive(true);
            
        }
        else if (currentBattery == 2)
        {
            batteryHalf.SetActive(true);
           
        }
        else if (currentBattery == 1)
        {
            batteryEmpty.SetActive(true);
            
        }
        else if (currentBattery == 0)
        {
            Bateria2.SetActive(true);

        }
    }
}