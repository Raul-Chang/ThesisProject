// PlayerGoal.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerGoal : MonoBehaviour
{
    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null) GameManager.Instance.PlayerReachedGoal();

        // Optionally play feedback (sound, light) on the goal object
    }
}
