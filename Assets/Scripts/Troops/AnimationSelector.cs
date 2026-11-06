using UnityEngine;

public class AnimationSelector : MonoBehaviour
{

    [SerializeField]private Animator m_Animator;
    [SerializeField]private ATroop _troop;
    [SerializeField]private TroopDefinition _troopDefinition;
    public int select;
    

    private void Start()
    {

        m_Animator = GetComponent<Animator>();
        _troop = GetComponent<ATroop>();
        _troopDefinition = _troop.GetDefinition();
        select = (int)_troopDefinition.type +1;
        m_Animator.SetInteger("Troop", select);
        
    }



}
