using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISticksCounter : MonoBehaviour , IObserver<GameEvent<ResourceSlot>> {


    private TextMeshProUGUI text;
    private int UIresources = 0;

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if(data.logicEvent == LogicEvent.RESOURCE_GATHER_2)
        text.text = "Sticks: " + (UIresources + data.data.cantidad).ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     text = this.GetComponent<TextMeshProUGUI>();
        text.text = "Sticks: " + UIresources.ToString();
    }

}
