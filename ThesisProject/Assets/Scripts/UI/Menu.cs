using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static Menu Instance { get; private set; }

    public static bool IsPaused { get; private set; } = false;

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       // DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        // Test Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None; // unlock mouse
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked; // lock mouse again
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        // Reloads the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game pressed!");

        // Works in a built game (not in the editor)
        Application.Quit();

        #if UNITY_EDITOR
        // Stops play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}




