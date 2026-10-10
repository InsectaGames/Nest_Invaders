using UnityEngine;
using TMPro;

public class UISticksCounter : MonoBehaviour, IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;

    void Start()
    {

        text = GetComponent<TextMeshProUGUI>();

    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if(data.logicEvent == LogicEvent.RESOURCE_GATHER_2)
        text.text = "Sticks: " + data.data.cantidad.ToString();
    }

    
}