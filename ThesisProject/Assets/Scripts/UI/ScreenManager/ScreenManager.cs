using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    private Stack<IScreen> _screensStack;

    [SerializeField] private MainScreen _mainScreen;

    [SerializeField] private ShopScreen _shopScreen;

    [SerializeField] private LvlSelectScreen _lvlSelectScreen;

    [SerializeField] private OptionsScreen _optionScreen;

    [SerializeField] private ControlsScreen1 _controlScreen;

    private void Awake()
    {
        Instance = this;

        _screensStack = new Stack<IScreen>();
    }

    public void Push(IScreen newScreen)
    {
        if (_screensStack.Contains(newScreen)) return;

        if (_screensStack.Count != 0)
        {
            var oldScreen = _screensStack.Peek();
            oldScreen.Deactivate();
        }

        _screensStack.Push(newScreen);
        newScreen.Activate();
    }
    
    public void Pop()
    {
        if (_screensStack.Count <= 0) return;

        
        var screenToPop = _screensStack.Pop();
        screenToPop.Release();

        var lastScreen = _screensStack.Peek();
        lastScreen.Activate();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ScreenManager.Instance.Pop();
        }
    }



    public void ShowOption()
    {
        Push(_optionScreen);
    }

    public void ShowControls()
    {
        Push(_controlScreen);
    }
    public void ShowShop()
    {
        Push(_shopScreen);
    }
    public void ShowLvlSelect()
    {
        Push(_lvlSelectScreen);
    }
}