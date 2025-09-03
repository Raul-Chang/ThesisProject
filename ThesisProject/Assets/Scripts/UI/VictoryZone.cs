using UnityEngine;
using TMPro;

public class VictoryZone : MonoBehaviour
{
    [Header("References")]
    public CountdownTimer countdownTimer; // Link your existing countdown timer
    public GameObject victoryText;   // Drag the "Victory!!!" text
    public GameObject defeatText;    // Drag the "Defeat!" text

    private bool playerInside = false;
    private bool resultShown = false;

    void Start()
    {
        if (victoryText != null) victoryText.SetActive(false);
        if (defeatText != null) defeatText.SetActive(false);
    }

    void Update()
    {
        if (!resultShown && countdownTimer != null && !countdownTimer.IsRunning() && countdownTimer.GetTimeRemaining() <= 0)
        {
            if (playerInside)
                ShowVictoryMessage();
            else
                ShowDefeatMessage();

            resultShown = true; // prevent multiple triggers
        }
    }

    private void ShowVictoryMessage()
    {
        if (victoryText != null)
        {
            victoryText.SetActive(true);
            Menu.Instance.PauseGame();
            Debug.Log("Victory!!!");
        }
    }

    private void ShowDefeatMessage()
    {
        if (defeatText != null)
        {
            defeatText.SetActive(true);
            Menu.Instance.PauseGame();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}
