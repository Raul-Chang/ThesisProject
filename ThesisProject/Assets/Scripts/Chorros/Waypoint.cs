using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Waypoint[] connectedWaypoints;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var wp in connectedWaypoints)
        {
            if (wp != null)
                Gizmos.DrawLine(transform.position, wp.transform.position);
        }
    }
}