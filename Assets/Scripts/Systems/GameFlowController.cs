using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Estado de juego")]
    [SerializeField] private GameObject[] pauseMenu;
    private bool gamePaused = false;
    public bool GamePaused => gamePaused;

    [Header("Victoria y Derrota")]
    [SerializeField] private string victoryScene;
    [SerializeField] private string defeatScene;

    [Header("Niveles")]
    [SerializeField] private List<string> gameLevels;
    private string currentLevel;

    private void Update()
    {
        if(!gameLevels.Contains(SceneManager.GetActiveScene().name))
            return;

        if(Utils.KeyPressed(UnityEngine.InputSystem.Key.Escape))
        {
            if(gamePaused)
                ResumeGame();
            else
                PauseGame();
        }  
    }

    #region MENU

    public void StartGame()
    {
        LoadLevel(gameLevels[0]);
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }

    #endregion

    #region NIVELES

    public void LoadLevel(string name)
    {
        if(FindLevel(name))
        {
            currentLevel = name;
            SceneManager.LoadScene(name);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        gamePaused = false;

        pauseMenu = Array.ConvertAll(FindObjectsByType<Transform>
            (FindObjectsInactive.Include, FindObjectsSortMode.None), t => t.gameObject);

        pauseMenu = Array.FindAll(pauseMenu, obj => obj.CompareTag("Pause"));

        Debug.Log($"Elementos del menú de pausa encontrados: {pauseMenu.Length}");
    }

    private bool FindLevel(string name)
    {
        foreach(string level in gameLevels)
        {
            if(level.Equals(name))
            {
                return true;
            }
        }

        return false;
    }

    public void RestartLevel()
    {
        if(!String.IsNullOrEmpty(currentLevel))
        {
            SceneManager.LoadScene(currentLevel);
        }
    }

    public void LoadNextLevel()
    {
        int pos = gameLevels.IndexOf(currentLevel);
        if(pos != -1 && pos < gameLevels.Count - 1)
        {
            SceneManager.LoadScene(gameLevels[pos + 1]);
        }
    }

    #endregion

    #region PAUSA

    public void PauseGame()
    {
        if(!gamePaused)
        {
            gamePaused = true;
            foreach(GameObject elem in pauseMenu ?? Array.Empty<GameObject>())
            {
                elem?.SetActive(true);
            }
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        if(gamePaused)
        {
            gamePaused = false;
            foreach(GameObject elem in pauseMenu ?? Array.Empty<GameObject>())
            {
                elem?.SetActive(false);
            }
            Time.timeScale = 1;
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    #endregion

    #region VICTORIA Y DERROTA

    public void TriggerVictory()
    {
        if(victoryScene != null)
        {
            SceneManager.LoadScene(victoryScene);
        }
        else
        {
            Debug.LogWarning("No hay una escena de victoria asignada.");
        }
    }

    public void TriggerDefeat()
    {
        if(defeatScene != null)
        {
            SceneManager.LoadScene(defeatScene);
        }
        else
        {
            Debug.LogWarning("No hay una escena de derrota asignada.");
        }
    }

    #endregion
}