using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;

    void LateUpdate()
    {
        // Smoothly follow instead of snapping
        Vector3 targetPos = player.position;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
