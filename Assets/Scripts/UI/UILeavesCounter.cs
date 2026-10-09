using TMPro;
using UnityEngine;

public class UILeavesCounter : MonoBehaviour ,IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;
    private int UIresources = 0;

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        text.text = "Leaves: " + (UIresources + data.data.cantidad).ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

       

        text = this.GetComponent<TextMeshProUGUI>();
        text.text = "Leaves: " + UIresources.ToString();
    }

   
  
  
}
