using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Tilemaps;

public class TroopsManager : MonoBehaviour
{
  
    public List<GameObject> EntityList = new List<GameObject>();
    public List<GameObject> ObserversList = new List<GameObject>();
    public  GameObject[] UIResources;
   

    public void AddTroop(GameObject go)
    {

        
        EntityList.Add(go);
        if (go.GetComponent<Troop>().GetDefinition().type == TroopType.GATHERER)
        {
            ObserversList.Add(go);
            go.GetComponent<Troop>().AddObserver(UIResources[0].GetComponent<UILeavesCounter>());
            go.GetComponent<Troop>().AddObserver(UIResources[1].GetComponent<UISticksCounter>());
        }
    }

    public void RemoveTroop(GameObject go)
    {
        EntityList.Remove(go);
        if (go.GetComponent<Troop>().GetDefinition().type == TroopType.GATHERER)
        {
            ObserversList.Add(go);
            go.GetComponent<Troop>().RemoveObserver(UIResources[0].GetComponent<UILeavesCounter>());
            go.GetComponent<Troop>().RemoveObserver(UIResources[1].GetComponent<UISticksCounter>());
        }

    }


    

}
