using TMPro;
using UnityEngine;

public class UIFungusCounter : MonoBehaviour, IObserver<int>
{
    private TextMeshProUGUI text;
    BaseSystem bs;


    public void UpdateObserver(int data) { 
   
      
            text.text = "Fungus: " + data.ToString();
        
    }

    private void Start()
    {
    
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Base"))
        {
            bs = obj.GetComponent<BaseSystem>();
            if (bs.GetFaction() == Faction.ALLY)
            {
                bs.AddObserver(this);
                Debug.Log("He he suscrito a:" + bs.gameObject.name);
            }
        }
    
     }
}