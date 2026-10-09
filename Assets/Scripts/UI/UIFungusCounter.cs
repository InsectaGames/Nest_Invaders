using TMPro;
using UnityEngine;

public class UIFungusCounter : MonoBehaviour ,IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;
    private int UIresources = 0;

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if (data.logicEvent == LogicEvent.RESOURCE_GATHER_1)
            text.text = "Fungus: " + (UIresources + data.data.cantidad).ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

       

        text = this.GetComponent<TextMeshProUGUI>();
        text.text = "Fungus: " + UIresources.ToString();
    }

   
  
  
}
