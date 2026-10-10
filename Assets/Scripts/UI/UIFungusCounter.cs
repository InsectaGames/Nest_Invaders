using TMPro;
using UnityEngine;

public class UIFungusCounter : MonoBehaviour, IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;
    private int UIresources;

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if (data.logicEvent == LogicEvent.RESOURCE_GATHER_1)
        {
            UIresources += data.data.cantidad;
            text.text = "Fungus: " + UIresources.ToString();
        }
        else if (data.logicEvent == LogicEvent.TROOP_PLACED)
        {
            UIresources -= data.data.cantidad;
            text.text = "Fungus: " + UIresources.ToString();
        }
    }

    private void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();

        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Base"))
        {
            BaseSystem bs = obj.GetComponent<BaseSystem>();
            if(bs.GetFaction() == Faction.ALLY)
            {
                UIresources = bs.GetFungus();
            }
        }
    }
}