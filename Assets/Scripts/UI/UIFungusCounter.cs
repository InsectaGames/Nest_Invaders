using TMPro;
using UnityEngine;

public class UIFungusCounter : MonoBehaviour, IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;

  void Awake()
    {

        text = GetComponent<TextMeshProUGUI>();

    }

    public void UpdateObserver(GameEvent<ResourceSlot> data) {

        if (data.logicEvent == LogicEvent.RESOURCE_GATHER_1)
            text.text = "Fungus: " + data.data.cantidad.ToString();
        
    }

  
}