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
    [SerializeField] private LevelSelectionAnimator selectionAnimator;
    [SerializeField] private string unlockAnimationState;

    [Header("Camino de desbloqueo")]
    [SerializeField] private GameObject pathParent;
    [SerializeField] private Sprite lockedPathSprite;
    [SerializeField] private Sprite unlockedPathSprite;

    private Image[] pathImages;

    private bool isUnlocked;

    private void Awake()
    {
        if (pathParent != null)
            pathImages = pathParent.GetComponentsInChildren<Image>(true);
    }

    private void Start()
    {
        RefreshState();

        bool pendingAnimation = LevelProgression.ConsumePendingAnimation(levelDefinition.ID);

        if (pendingAnimation && selectionAnimator != null)
        {
            selectionAnimator.PlayUnlockAnimation(unlockAnimationState);
        }
    }

    public void RefreshState()
    {
        isUnlocked = LevelProgression.IsUnlocked(levelDefinition, unlockedAtStart);

        if (button != null)
            button.interactable = isUnlocked;

        if (buttonImage != null)
            buttonImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;

        if (pathImages != null)
        {
            foreach (Image pathImage in pathImages)
            {
                pathImage.sprite = isUnlocked ? unlockedPathSprite : lockedPathSprite;
            }
        }
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