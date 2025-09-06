using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapPlayerUI : MonoBehaviour
{
    public Camera minimapCamera;        
    public RectTransform minimapRect;   
    public RectTransform iconRect;      
    public Transform player;            

    void Update()
    {
        if (minimapCamera == null || minimapRect == null || iconRect == null || player == null) return;

       
        Vector3 vp = minimapCamera.WorldToViewportPoint(player.position);

        
        if (vp.z < 0f)
        {
            iconRect.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!iconRect.gameObject.activeSelf) iconRect.gameObject.SetActive(true);
        }

        
        Vector2 mapSize = minimapRect.rect.size;
        Vector2 localPos;
        localPos.x = (vp.x - 0.5f) * mapSize.x;
        localPos.y = (vp.y - 0.5f) * mapSize.y;

        iconRect.anchoredPosition = localPos;

       
        iconRect.localEulerAngles = new Vector3(0f, 0f, -player.eulerAngles.y);
    }
}