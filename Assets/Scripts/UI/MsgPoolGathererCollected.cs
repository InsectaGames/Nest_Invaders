
using System.Collections;
using UnityEngine;

public class MsgPoolGathererCollected : MonoBehaviour,
    IObserver<GameEvent<ResourceSlot>>
{
    private IObjectPool<MsgGathered> pool;

    [SerializeField] private MsgGathered prefab;
    [SerializeField, Min(1)] private int initialSize = 10;
    [SerializeField] private Transform parent;

    private void Awake()
    {
        pool = new AObjectPool<MsgGathered>(
            prefab,
            initialSize,
            parent
        );
    }

   void Start()
    {

       // yield return null;

        GameObject[] _go = GameObject.FindGameObjectsWithTag("Base");

        foreach (var go in _go)
        {
            if (go.GetComponent<BaseSystem>().GetFaction() == Faction.ALLY)
            {
                go.GetComponent<BaseSystem>().AddObserver(this);
                Debug.Log("Me suscribi a: " + go.name);
            }
        }
    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if (data.logicEvent != LogicEvent.RESOURCE_GATHERED_1)
            return;

        MsgGathered msg = pool.Get();

        if (msg == null)
            return;

        msg.Initialize(ReleaseMessage);
        msg.RecieveQuantity(data.data.cantidad);
    }

    private void ReleaseMessage(MsgGathered msg)
    {
        pool.Release(msg);
    }
}
