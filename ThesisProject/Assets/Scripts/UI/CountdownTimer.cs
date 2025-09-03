using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTimeInSeconds = 20f; 
    private float currentTime;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    private bool isRunning = true;

    void Start()
    {
        currentTime = startTimeInSeconds;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                // You can call a method here when timer hits zero
                Debug.Log("Timer finished!");
            }

            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetTimeRemaining()
    {
        return currentTime;
    }

}
