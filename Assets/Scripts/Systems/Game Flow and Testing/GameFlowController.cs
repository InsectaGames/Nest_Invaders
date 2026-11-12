using System;
using System.Collections;
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
    [SerializeField] private string creditsScene = "Credits";
    [SerializeField] private string settingsScene = "Settings";
    [SerializeField] private string shopScene = "Shop";

    private string _TransSceneSetted;


    [Header("Transicion entre escenas")]
    [SerializeField] private Animator transition;


    private void Awake()
    {
        transition = this.GetComponent<Animator>();
    }

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

        CallSceneChange();
        SetSceneChange(shopScene);
    }
    
    public void GoToSettings()
    {
        Debug.Log("--- SETTINGS ---");
        Debug.Log("Instance ID: " + GetInstanceID());
        Debug.Log("Active In Hierarchy: " + gameObject.activeInHierarchy);
        Debug.Log("Enabled: " + isActiveAndEnabled);


        CallSceneChange();
        SetSceneChange(settingsScene);

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

        CallSceneChange();
        SetSceneChange(gameplayScene);
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

        CallSceneChange();
        SetSceneChange(gameplayScene);
    }

    public void ReturnToLevelSelection()
    {
        Time.timeScale = 1f;
        gamePaused = false;

        CallSceneChange();
        SetSceneChange(levelSelectionScene);
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
        CallSceneChange();
        SetSceneChange("Menu");
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

    #region CREDITOS

    public void GoToCredits()
    {
        CallSceneChange();
        SetSceneChange(creditsScene);

    }

  
    #endregion

    public void CallSceneChange()
    {
     
        transition.SetTrigger("in");

    }

    private void SetSceneChange(string scene)
    {
        _TransSceneSetted = scene;
    }
    public void TransitionEnded()
    {
 
        SceneManager.LoadScene(_TransSceneSetted);
    }
}