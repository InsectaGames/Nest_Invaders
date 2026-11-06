using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : MonoBehaviour
{
    [Header("Nivel")]
    [SerializeField] private LevelDefinition levelDefinition;
    [SerializeField] private bool unlockedAtStart;

    [Header("Visual del botón")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Button button;

    [Header("Animación de desbloqueo")]
    [SerializeField] private Animator animator;
    [SerializeField] private string unlockAnimationState;

    private bool isUnlocked;

    private void Start()
    {
        RefreshState();

        if (LevelProgression.ConsumePendingAnimation(levelDefinition.ID))
        {
            if (animator != null && !string.IsNullOrEmpty(unlockAnimationState))
                animator.Play(unlockAnimationState, 0, 0f);
        }
    }

    public void RefreshState()
    {
        isUnlocked = LevelProgression.IsUnlocked(levelDefinition, unlockedAtStart);

        if (button != null)
            button.interactable = isUnlocked;

        if (buttonImage != null)
            buttonImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
    }

    public void SelectLevel()
    {
        if (!isUnlocked)
            return;

        GameFlowController gameFlow = FindFirstObjectByType<GameFlowController>();

        if (gameFlow != null)
            gameFlow.LoadSelectedLevel(levelDefinition);
    }
}