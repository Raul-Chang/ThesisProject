using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraFollow : MonoBehaviour
{
    [Header("Target (opcional)")]
    public Transform target;
    public Vector3 offset = new Vector3(-19, 253, 14);

    [Header("Opciones")]
    public bool followTarget = true;

    void LateUpdate()
    {
        if (followTarget && target != null)
        {
           
            transform.position = target.position + offset;
        }

     
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}