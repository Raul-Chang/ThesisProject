using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManagerGame : MonoBehaviour
{
    public static ScreenManagerGame Instance { get; private set; }

    private Stack<IScreen> _screensStack;

    [SerializeField] private MainScreen _mainScreen;

    [SerializeField] private MenuScreen _menuScreen;

    [SerializeField] private OptionsScreen _optionScreen;


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
        
        if (_screensStack.Count <= 1) return;


        var screenToPop = _screensStack.Pop();
        screenToPop.Release();

        var lastScreen = _screensStack.Peek();
        lastScreen.Activate();


        if (_screensStack.Count == 1)
        {
            Cursor.lockState = CursorLockMode.Locked; // lock mouse again
            Cursor.visible = false;
        }


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
          
            Cursor.lockState = CursorLockMode.None; // unlock mouse
            Cursor.visible = true;
            ShowMenu();

        }

    }



    public void ShowMenu()
    {
        if (_screensStack.Count <= 1)
        {
            Push(_menuScreen);
        }
        else
        {
            Pop();
        }
    }

    public void ShowOptions()
    {
        
            Push(_optionScreen);
            
    }
}

