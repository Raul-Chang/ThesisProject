// GameManager.cs
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Day / Night settings")]
    public int totalDays = 5;
    [Tooltip("Seconds per night (3 minutes = 180)")]
    public float nightDurationSeconds = 180f;
    [Tooltip("Delay between days (seconds)")]
    public float betweenDayDelay = 2f;

    [Header("References")]
    public WeatherController weatherController;
    public Transform playerTransform;
    public Transform playerStartTransform; // where to respawn player at next day start (optional)

    // runtime
    int currentDayIndex = 0;
    bool reachedGoal = false;
    bool caught = false;
    Coroutine nightCoroutine = null;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (weatherController == null) Debug.LogWarning("WeatherController not assigned in GameManager.");
        if (weatherController != null) weatherController.GenerateForecast();

        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(0.1f);
        StartDay(currentDayIndex);
    }

    public void StartDay(int dayIndex)
    {
        currentDayIndex = Mathf.Clamp(dayIndex, 0, totalDays - 1);
        reachedGoal = false;
        caught = false;

        // Apply weather for this day
        if (weatherController != null) weatherController.ApplyForecastDay(currentDayIndex);

        // reset player position if assigned
        if (playerTransform != null && playerStartTransform != null)
        {
            playerTransform.position = playerStartTransform.position;
            playerTransform.rotation = playerStartTransform.rotation;
            // optional: reset player controller state here
        }

        // start night timer
        if (nightCoroutine != null) StopCoroutine(nightCoroutine);
        nightCoroutine = StartCoroutine(NightTimerRoutine());
    }

    IEnumerator NightTimerRoutine()
    {
        float timeLeft = nightDurationSeconds;

        while (timeLeft > 0f)
        {
            // if player was caught, immediate fail
            if (caught)
            {
                EndDay(false, "Caught by enemies");
                yield break;
            }

            // otherwise let timer run; reaching goal doesn't end timer - they must wait until it runs out
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // timer ended - result depends on whether player reached goal
        if (reachedGoal && !caught)
        {
            EndDay(true, "Reached target before night ended (waited until night end)");
        }
        else
        {
            EndDay(false, "Time expired before reaching target");
        }
    }

    // Called from PlayerGoal when player reaches the waypoint
    public void PlayerReachedGoal()
    {
        if (caught) return; // already lost
        if (reachedGoal) return;
        reachedGoal = true;
        Debug.Log("Player reached goal - must wait until night ends to succeed.");
        // Optionally provide audio/UI feedback here.
    }

    // Called from an enemy detection script
    public void PlayerCaught()
    {
        if (caught) return;
        caught = true;
        Debug.Log("Player was caught by enemies - immediate failure.");
    }

    void EndDay(bool success, string reason)
    {
        Debug.Log($"Day {currentDayIndex + 1} ended. Success: {success}. Reason: {reason}");

        // Stop active timers
        if (nightCoroutine != null) StopCoroutine(nightCoroutine);
        nightCoroutine = null;

        // process results: maybe play sounds, stats, show UI etc.

        // advance day
        StartCoroutine(AdvanceDayAfterDelay(success));
    }

    IEnumerator AdvanceDayAfterDelay(bool previousDayWasSuccess)
    {
        // optional logic based on success/fail can go here (reward/penalty)
        yield return new WaitForSeconds(betweenDayDelay);

        currentDayIndex++;
        if (currentDayIndex >= totalDays)
        {
            Debug.Log("All days complete. End of scenario.");
            // handle scenario end (victory/defeat screen) -- placeholder
            // For now, loop back or stop
            currentDayIndex = 0; // or stop further progression
            // weatherController.GenerateForecast(); // if you want new forecast
        }

        StartDay(currentDayIndex);
    }
}
