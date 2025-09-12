using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : MonoBehaviour, IScreen
{
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = false;
    }

    public void Activate()
    {

        _canvas.enabled = true;
    }

    public void Deactivate()
    {
        _canvas.enabled = false;
    }

    public void Release()
    {
        _canvas.enabled = false;
    }
}
