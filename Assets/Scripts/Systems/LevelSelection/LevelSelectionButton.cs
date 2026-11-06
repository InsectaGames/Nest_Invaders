using UnityEngine;

public class LevelSelectionButton : MonoBehaviour
{
    [Header("Nivel asociado")]
    [SerializeField] private LevelDefinition levelDefinition;

    [Header("Disponibilidad temporal")]
    [SerializeField] private bool unlockedAtStart = false;

    private bool isLoading = false;

    public void SelectLevel()
    {
        if (isLoading)
            return;

        if (levelDefinition == null)
        {
            Debug.LogError($"No hay LevelDefinition asignado a {gameObject.name}.");
            return;
        }

        if (!unlockedAtStart)
        {
            Debug.Log($"El nivel {levelDefinition.Name} está bloqueado.");
            return;
        }

        GameFlowController gameFlow = FindFirstObjectByType<GameFlowController>();

        if (gameFlow == null)
        {
            Debug.LogError("No se ha encontrado GameFlowController en SubLevelsScene.");
            return;
        }

        isLoading = true;

        gameFlow.LoadSelectedLevel(levelDefinition);
    }

    // Alternativa para carteles que utilizan SpriteRenderer y Collider2D.
    private void OnMouseDown()
    {
        SelectLevel();
    }
}