using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : Singleton<GameFlowController>
{
    [Header("Estado de juego")]
    private bool gamePaused = false;

    [Header("Victoria y Derrota")]
    [SerializeField] private string victoryScene;
    [SerializeField] private string defeatScene;

    [Header("Niveles")]
    [SerializeField] private List<string> gameLevels;
    private string currentLevel;

    #region MENU
    public void StartGame()
    {
        LoadLevel(gameLevels[0]);
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
            SceneManager.LoadScene(name);
        }
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
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        if(gamePaused)
        {
            gamePaused = false;
            Time.timeScale = 1;
        }
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