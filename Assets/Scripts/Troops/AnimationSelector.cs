using UnityEngine;

public class AnimationSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator m_Animator;
    
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

        select = definition.ID + 1;

        m_Animator.Rebind();
        m_Animator.SetInteger("Troop", select);
        m_Animator.Update(0f);
    }
}