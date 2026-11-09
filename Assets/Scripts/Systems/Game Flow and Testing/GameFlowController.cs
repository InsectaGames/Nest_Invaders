using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Estado de juego")]
    [SerializeField] private GameObject pauseMenu;
    private bool gamePaused = false;
    public bool GamePaused => gamePaused;

    [Header("Escenas")]
    [SerializeField] private string levelSelectionScene = "SubLevelsScene";
    [SerializeField] private string gameplayScene = "Main_arena";
    [SerializeField] private string victoryScene = "VictoryScene";
    [SerializeField] private string defeatScene = "DefeatScene";

    private void Update()
    {
        // Debug.Log("Estoy activo : " + this.isActiveAndEnabled);
        if(SceneManager.GetActiveScene().name != gameplayScene && !GameTestMode.IsBehaviourTestScene)
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
        ReturnToLevelSelection();
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

    public void LoadSelectedLevel(LevelDefinition level)
    {
        if (level == null)
        {
            Debug.LogError("No se ha asignado una definición de nivel.");
            return;
        }

        if (string.IsNullOrEmpty(gameplayScene))
        {
            Debug.LogError("No se ha configurado la escena de Gameplay.");
            return;
        }

        LevelSession.SelectLevel(level);

        Time.timeScale = 1f;
        gamePaused = false;

        Debug.Log($"Cargando nivel: {level.Name} (ID: {level.ID})");

        SceneManager.LoadScene(gameplayScene);
    }

    public void RestartLevel()
    {
        if (GameTestMode.IsBehaviourTestScene)
        {
            Time.timeScale = 1f;
            gamePaused = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (LevelSession.SelectedLevel == null)
        {
            Debug.LogWarning("No hay ningún nivel seleccionado para reiniciar.");
            ReturnToLevelSelection();
            return;
        }

        Time.timeScale = 1f;
        gamePaused = false;

        SceneManager.LoadScene(gameplayScene);
    }

    public void ReturnToLevelSelection()
    {
        Time.timeScale = 1f;
        gamePaused = false;

        SceneManager.LoadScene(levelSelectionScene);
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

        GameObject[] objects = Array.ConvertAll(FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None), t => t.gameObject);
        
        pauseMenu = Array.Find(objects, obj => obj.CompareTag("Pause"));

        if(pauseMenu != null)
            Debug.Log($"Elementos del menú de pausa encontrados: {pauseMenu.name}");
    }

    /*
    private IEnumerator ChangeSceneTrans(string SceneName)
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
        Time.timeScale = 1f;
        gamePaused = false;
        AudioManager.Instance?.StopBackgroundMusic();
        SceneManager.LoadScene("Menu");
    }

    #endregion

    #region VICTORIA Y DERROTA

    public void TriggerVictory()
    {
        // En la arena de pruebas no se cambia de escena.
        if (GameTestMode.IsBehaviourTestScene)
        {
            Debug.Log("Victoria detectada en modo de pruebas. Permanecemos en la escena.");
            return;
        }

        Debug.Log("¡Nivel completado!");

        LevelDefinition completedLevel = LevelSession.SelectedLevel;

        if (completedLevel == null)
        {
            Debug.LogError("No se puede registrar la victoria: no hay ningún nivel seleccionado.");
            ReturnToLevelSelection();
            return;
        }

        bool nextLevelUnlocked = LevelProgression.CompleteLevel(completedLevel);

        if (nextLevelUnlocked)
        {
            Debug.Log($"¡Nuevo nivel desbloqueado! {completedLevel.nextLevel.Name}");
        }
        else if (completedLevel.nextLevel != null)
        {
            Debug.Log($"El siguiente nivel ya estaba desbloqueado: {completedLevel.nextLevel.Name}");
        }


        SceneManager.LoadScene(victoryScene, LoadSceneMode.Additive);
       // ReturnToLevelSelection();
    }


    public void TriggerDefeat()
    {
        if (GameTestMode.IsBehaviourTestScene)
        {
            Debug.Log("Derrota detectada en modo de pruebas. Permanecemos en la escena.");
            return;
        }

        Time.timeScale = 1f;
        gamePaused = false;

        if (!string.IsNullOrEmpty(defeatScene))
        {
            SceneManager.LoadScene(defeatScene , LoadSceneMode.Additive);
        }
        else
        {
            ReturnToLevelSelection();
        }
    }

    #endregion
}