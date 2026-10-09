
using UnityEngine;
using System.Collections.Generic;

public class TroopsManager : MonoBehaviour
{
    public static TroopsManager Instance { get; private set; }

    [Header("Entity Registry")]
    public List<GameObject> EntityList = new List<GameObject>();
    public List<GameObject> ObserversList = new List<GameObject>();

    [Header("UI Resource Counters")]
    [SerializeField] private GameObject[] UIResources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddTroop(GameObject go)
    {
        if (go == null || EntityList.Contains(go))
            return;

        Troop troop = go.GetComponent<Troop>();

        if (troop == null)
        {
            Debug.LogWarning($"{go.name} no tiene componente Troop.");
            return;
        }

        EntityList.Add(go);

        if (troop.GetDefinition().type != TroopType.GATHERER)
            return;

        ObserversList.Add(go);

        if (UIResources == null || UIResources.Length < 2)
        {
            Debug.LogWarning("Faltan referencias a los contadores de recursos.");
            return;
        }

        UIFungusCounter fungusCounter =
            UIResources[0].GetComponent<UIFungusCounter>();

        UISticksCounter sticksCounter =
            UIResources[1].GetComponent<UISticksCounter>();

        if (fungusCounter != null)
            troop.AddObserver(fungusCounter);

        if (sticksCounter != null)
            troop.AddObserver(sticksCounter);
    }


    public void RemoveTroop(GameObject go)
    {
        if (go == null)
            return;

        EntityList.Remove(go);
        ObserversList.Remove(go);

        Troop troop = go.GetComponent<Troop>();

        if (troop == null)
            return;

      
        if (UIResources == null || UIResources.Length < 2)
            return;

        GameObject fungusUI = UIResources[0];
        GameObject sticksUI = UIResources[1];

        if (fungusUI != null)
        {
            UIFungusCounter fungusCounter =
                fungusUI.GetComponent<UIFungusCounter>();

            if (fungusCounter != null)
                troop.RemoveObserver(fungusCounter);
        }

        if (sticksUI != null)
        {
            UISticksCounter sticksCounter =
                sticksUI.GetComponent<UISticksCounter>();

            if (sticksCounter != null)
                troop.RemoveObserver(sticksCounter);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}