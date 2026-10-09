using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public struct TowerSummon
{
    public TroopDefinition towerInfo;
    public Key key;
}

public struct TroopInstance
{
    public GameObject go;
    public int id;
}

public class BaseSystem : MonoBehaviour, IObserver<GameEvent<int>>, IObserver<GameEvent<ResourceSlot>>
{
    [Header("Configuración")]
    [SerializeField] private Faction faction;
    [SerializeField] private int HP = 3;
    private HealthComponent health;

    [Header("Recursos")]
    [SerializeField] private ResourceSlot[] resources;
    [SerializeField] private int[] maxResources;
    

    [Header("Lógica de Spawner")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<TowerSummon> towerSummons;
    private Mutex spawnerMutex = new Mutex();

    private void Start()
    {
        
        health = gameObject.GetComponent<HealthComponent>();
        health?.Initialize(HP);

        switch(faction)
        {
            case Faction.ALLY:
                UITroopsGenerator generator = FindFirstObjectByType<UITroopsGenerator>();

                if (generator == null)
                {
                    Debug.LogError("No se encontró UITroopsGenerator en la escena.");
                    return;
                }

                generator.AddObserver(this);
            break;

            case Faction.ENEMY:
                // StartCoroutine(SpawnCoroutine());
            break;
        }
    }

    private void Update()
    {
        if(health.IsDead())
        {
            GameFlowController gfc = GameObject.FindGameObjectWithTag("GameFlowController").GetComponent<GameFlowController>();
            if (gfc != null)
            {
                switch (faction)
                {
                    case Faction.ALLY:
                        gfc.TriggerDefeat();
                    break;
                    
                    case Faction.ENEMY:
                        StopCoroutine(SpawnCoroutine());
                        gfc.TriggerVictory();
                    break;
                }
            }
            else
            {
                Debug.LogError("No se encontró el GameFlowController en la escena. Asegúrate de que el Tag sea correcto.");
            }

            health.Die();
        }
        
        if(faction == Faction.ALLY)
        {
            for(int i = 0; i < towerSummons.Count; i++)
            {
                if (Utils.KeyPressed(towerSummons[i].key)) SpawnUnit(i);   
            }
        }
    }

    public void SpawnUnit(int id = 0)
    {
        spawnerMutex.WaitOne();
        Debug.Log("Colocando torre...");

        if(id < 0 || id > towerSummons.Count)
        {
            Debug.LogWarning("ID fuera de los límites de la lista.");
            return;

        }
        else if (prefab == null)
        {
            Debug.LogWarning("No hay un prefab de torre asignado para colocar.");
            return;
        }

        Troop troop = Instantiate(prefab).GetComponent<Troop>();
        troop.Initialize(towerSummons[id].towerInfo);
        resources[0].cantidad--;
        Debug.Log($"Torre colocada: { troop.GetDefinition().Name }, ahora tengo { resources[0].cantidad } hojas.");

        spawnerMutex.ReleaseMutex();
    }

    private IEnumerator SpawnCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);

            SpawnUnit();   
        }
    }

    public Faction GetFaction() { return this.faction; }

    public bool IsDestroyed() => health.IsDead();

    #region PATRÓN OBSERVER
    public void UpdateObserver(GameEvent<int> data)
    {
        if(data.logicEvent == LogicEvent.TROOP_PLACED && faction == Faction.ALLY)
        {
            SpawnUnit(data.data);
        }
    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        Debug.Log("Info de recurso recibida!");
        if(data.logicEvent == LogicEvent.RESOURCE_GATHER_1 && faction == Faction.ALLY)
        {
            int pos = (int) data.data.res;
            resources[pos].cantidad = Utils.Clamp(++resources[pos].cantidad, 0, maxResources[pos]);
            Debug.Log($"Ahora tengo { resources[pos].cantidad } del recurso { resources[pos].res }");
        }
    }
    #endregion
}