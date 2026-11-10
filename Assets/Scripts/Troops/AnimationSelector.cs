using UnityEngine;

public class AnimationSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator m_Animator;
    
    [Header("Configuración visual")]
    [SerializeField] private TroopDefinition _troopDefinition;
    
    private int select;

    private void Awake()
    {
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();
    }

    public void Initialize(TroopDefinition definition)
    {
        if (definition == null || m_Animator == null)
            return;

        // Utilizar la definición alternativa si está configurada.
        TroopDefinition visualDefinition = _troopDefinition != null ? _troopDefinition : definition;

        select = visualDefinition.ID + 1;

        m_Animator.SetInteger("Troop", select);
    }
}