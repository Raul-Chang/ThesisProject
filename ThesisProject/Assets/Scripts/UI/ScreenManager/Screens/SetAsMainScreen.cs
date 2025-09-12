using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IScreen))]
public class SetAsMainScreen : MonoBehaviour
{
    void Start()
    {
        if (!TryGetComponent(out MainScreen mainScreen)) return;

        if(ScreenManager.Instance != null)
        ScreenManager.Instance.Push(mainScreen);
        else
        ScreenManagerGame.Instance.Push(mainScreen);    
    }
}
