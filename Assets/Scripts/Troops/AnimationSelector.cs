using UnityEngine;

public class AnimationSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator m_Animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int troopParameterHash;
    
    private int select;

    private void Awake()
    {
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        troopParameterHash = Animator.StringToHash("Troop");
    }

    public void ResetVisuals()
    {
        if (m_Animator != null)
        {
            m_Animator.enabled = false;
            m_Animator.Rebind();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.flipX = false;
        }
    }

    public void Initialize(TroopDefinition definition)
    {
        if (definition == null || m_Animator == null)
        {
            Debug.LogError($"{name}: no se puede inicializar la animación.");
            return;
        }

        m_Animator.enabled = true;
        m_Animator.Rebind();

        m_Animator.SetInteger(troopParameterHash, definition.ID + 1);
        m_Animator.Update(0f);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.enabled = true;
        }
    }
}