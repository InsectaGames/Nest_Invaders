using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Estado de juego")]
    [SerializeField] private GameObject pauseMenu;
    private bool gamePaused = false;
    public bool GamePaused => gamePaused;

    [Header("Victoria y Derrota")]
    [SerializeField] private string victoryScene;
    [SerializeField] private string defeatScene;

    [Header("Transicion")]
    [SerializeField] private Animator animatorTrans; 


    [Header("Niveles")]
    [SerializeField] private List<string> gameLevels;
    private string currentLevel;

    private void Awake()
    {
        animatorTrans = GameObject.FindGameObjectWithTag("Transition_canvas")
            .GetComponent<Animator>();

        Debug.Log("AWAKE - GameFlow ID: " + GetInstanceID());
        Debug.Log("AWAKE - Animator: " + animatorTrans);
    }


    private void Update()
    {

        //Debug.Log("Estoy activo : " + this.isActiveAndEnabled);
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


    public void ShopScene()
    {
        SceneManager.LoadScene("Shop");
    }
    public void GoToSettings()
    {
        SceneManager.LoadScene("Settings");
       // StartCoroutine(ChangeSceneTrans("Settings"));
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

        if(pauseMenu != null)
            Debug.Log($"Elementos del menú de pausa encontrados: {pauseMenu.name}");
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

/*
    IEnumerator ChangeSceneTrans(string SceneName)
    {
        Debug.Log("6. Dentro de la corrutina");

        animatorTrans.ResetTrigger("change");
        animatorTrans.SetTrigger("change");

        Debug.Log("7. Trigger enviado");

        yield return new WaitForSeconds(1.5f);

        Debug.Log("8. Cambio de escena");

        SceneManager.LoadScene(SceneName);
    }
*/

    #endregion

    #region PAUSA

    public void PauseGame()
    {
        if(!gamePaused)
        {
            gamePaused = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        if(gamePaused)
        {
            gamePaused = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void ReturnToMenu()
    {
        AudioManager.Instance?.StopBackgroundMusic();
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