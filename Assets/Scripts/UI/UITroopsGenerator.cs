using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITroopsGenerator : ASubject<GameEvent<int>>
{
    [SerializeField] private Button[] buttons;
    
    [Header("Control de invocación")]
    [SerializeField, Min(0.05f)]
    private float summonInputCooldown = 0.25f;

    private bool[] isSummoning;

    private void Awake()
    {
        isSummoning = new bool[buttons.Length];
    }

    public void UIGenerateEntity(int id)
    {
        if(id < 0 || id > buttons.Length || isSummoning[id])
            return;
        
        StartCoroutine(SummonEntity(id));
    }

    private IEnumerator SummonEntity(int id)
    {
        isSummoning[id] = true;
       // buttons[id].interactable = false;

        GameEvent<int> troopSummon = new GameEvent<int>(LogicEvent.TROOP_PLACED, id);

        UpdateObservers(troopSummon);

        yield return new WaitForSeconds(summonInputCooldown);

        isSummoning[id] = false;

        //if (buttons[id] != null)
        //  buttons[id].interactable = true;
    }
}