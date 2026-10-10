using UnityEngine;
using TMPro;

public class UISticksCounter : MonoBehaviour, IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;
    private int UIresources = 0;

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if(data.logicEvent == LogicEvent.RESOURCE_GATHER_2)
        text.text = "Sticks: " + (UIresources + data.data.cantidad).ToString();
    }

    private void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
        text.text = "Sticks: " + UIresources.ToString();
    }
}