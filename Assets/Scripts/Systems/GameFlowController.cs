using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController
{
    [Header("Estado de juego")]
    private LevelController levelController;
    private bool gamePaused = false;

    [Header("Victoria y Derrota")]
    [SerializeField] private Scene victoryScene;
    [SerializeField] private Scene defeatScene;

    [Header("Niveles")]
    [SerializeField] private List<Scene> gameLevels;
    private Scene currentLevel;

    public void StartGame()
    {
        LoadLevel(gameLevels[0].name);
    }

    public void LoadLevel(string name)
    {
        if(FindLevel(name))
        {
            SceneManager.LoadScene(name);
        }
    }

    private bool FindLevel(string name)
    {
        foreach(Scene level in gameLevels)
        {
            if(level.name.Equals(name))
            {
                return true;
            }
        }

        return false;
    }

    // Inicializar el nivel con todo lo necesario, la info la conoce el LevelController.
    public void StartLevel() => levelController.StartLevel();

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

    public void TriggerVictory()
    {
        if(victoryScene != null)
        {
            SceneManager.LoadScene(victoryScene.name);
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
            SceneManager.LoadScene(defeatScene.name);
        }
        else
        {
            Debug.LogWarning("No hay una escena de derrota asignada.");
        }
    }

    public void RestartLevel()
    {
        if(currentLevel != null)
        {
            SceneManager.LoadScene(currentLevel.name);
        }
    }

    public void LoadNextLevel()
    {
        int pos = gameLevels.IndexOf(currentLevel);
        if(pos != -1 && pos < gameLevels.Count - 1)
        {
            SceneManager.LoadScene(gameLevels[pos + 1].name);
        }
    }

}