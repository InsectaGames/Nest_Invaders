using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressionDebug : MonoBehaviour
{
    [SerializeField] private LevelDefinition[] levels;

    private void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD

        // F9: Reiniciar todo el progreso.
        if (Utils.KeyPressed(UnityEngine.InputSystem.Key.F9))
        {
            LevelProgression.ResetProgress(levels);
            ReloadScene();
        }

        // F10: Desbloquear todos los niveles.
        if (Utils.KeyPressed(UnityEngine.InputSystem.Key.F10))
        {
            UnlockAllLevels();
            ReloadScene();
        }

        #endif
    }

    private void UnlockAllLevels()
    {
        if (levels == null)
            return;

        foreach (LevelDefinition level in levels)
        {
            if (level == null)
                continue;

            LevelProgression.UnlockLevel(level);
        }

        Debug.Log("Todos los niveles han sido desbloqueados.");
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}