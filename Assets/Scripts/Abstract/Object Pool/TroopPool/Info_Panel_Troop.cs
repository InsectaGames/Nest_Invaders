using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Info_Panel_Troop : MonoBehaviour , IObserver<GameEvent<ATroop>>
{
    [SerializeField] private TMP_Text countTroop_text;
    [SerializeField] private Image countTroop_img;
    [SerializeField] private int countTroop;
    [SerializeField] private bool _isActive;
    [SerializeField] private TroopType self_Selector;
    [SerializeField] private Faction self_faction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GameObject.FindGameObjectWithTag("pool").GetComponent<TroopPoolManager>().AddObserver(this);
        countTroop_text.text = " x  0";
        countTroop = 0;
        _isActive = false;
        countTroop_img.gameObject.SetActive(_isActive);
        countTroop_text.gameObject.SetActive(_isActive);
    }
    
    public void UpdateObserver(GameEvent<ATroop> data)
    {
        if (data.data.GetDefinition().type != self_Selector)
            return;

        if (data.data.GetFaction() != self_faction)
            return;

        if(data.logicEvent == LogicEvent.TROOP_PLACED)
        {
            countTroop++;
            countTroop_text.text = " x " + countTroop;
        }
        else if(data.logicEvent == LogicEvent.TROOP_DESPAWN)
        {
            countTroop--;
            countTroop_text.text = " x " + countTroop;
        }

        if(countTroop <= 0)
        {
            _isActive = false;
            countTroop = 0;
        }
        else
        {
            _isActive = true;
        }
        
        countTroop_img.gameObject.SetActive(_isActive);
        countTroop_text.gameObject.SetActive(_isActive);
    }
}
