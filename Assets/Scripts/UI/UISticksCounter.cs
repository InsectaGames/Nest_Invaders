using UnityEngine;
using TMPro;

public class UISticksCounter : MonoBehaviour, IObserver<GameEvent<ResourceSlot>>
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        BaseSystem[] bases = FindObjectsByType<BaseSystem>(FindObjectsSortMode.None);

        foreach(BaseSystem b in bases)
        {
            if(b.GetFaction() == Faction.ALLY)
            {
                b.AddObserver(this);
            }
        }
    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if(data.logicEvent == LogicEvent.RESOURCE_GATHER_2 && data.data.res == Resource.STICK)
            text.text = data.data.cantidad.ToString();
    }
}