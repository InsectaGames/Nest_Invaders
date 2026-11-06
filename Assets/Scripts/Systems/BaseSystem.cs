using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

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

public class BaseSystem :ASubject<GameEvent<ResourceSlot>>, IObserver<GameEvent<int>>, IObserver<GameEvent<ResourceSlot>>
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
    [SerializeField] private TroopPoolManager troopPoolManager;
    [SerializeField] private List<TowerSummon> towerSummons;

    private Mutex spawnerMutex = new Mutex();

    public Faction GetFaction() => this.faction;

    public int GetLeaves() => GetResourceAmount(Resource.LEAVES);
    public int GetSticks() => GetResourceAmount(Resource.STICK);

    private int GetResourceAmount(Resource resource)
    {
        int index = GetResourceIndex(resource);

        if (index < 0)
            return 0;

        return resources[index].cantidad;
    }

    public bool IsDestroyed() => health.IsDead();

    private void Awake()
    {
        if (faction == Faction.ALLY)
            ApplyInitialResources();
    }

    private void Start()
    {
        if(troopPoolManager == null)
        {
            troopPoolManager = FindFirstObjectByType<TroopPoolManager>();    
        }

        health = gameObject.GetComponent<HealthComponent>();
        health?.Initialize(HP);

        Debug.Log("Soy el" + faction + "y entro en el switch");
        switch (faction)
        {
            case Faction.ALLY:
                UITroopsGenerator generator = FindFirstObjectByType<UITroopsGenerator>();
                Debug.Log("REGISTRANDO OBSERVER EN: " + generator?.GetInstanceID());
                generator?.AddObserver(this);
                break;

            case Faction.ENEMY:
                StartCoroutine(SpawnCoroutine());
                break;
        }
    }

    private void ApplyInitialResources()
    {
        if (faction != Faction.ALLY)
            return;

        LevelDefinition level = LevelSession.SelectedLevel;

        if (level == null)
        {
            Debug.LogError("BaseSystem: No hay ningún nivel seleccionado.", this);
            return;
        }

        if (level.initialResources == null)
        {
            Debug.LogWarning("BaseSystem: No hay recursos iniciales configurados.", this);
            return;
        }

        if (resources == null)
        {
            Debug.LogError("BaseSystem: El array de recursos no está inicializado.");
            return;
        }

        foreach (ResourceSlot initialResource in level.initialResources)
        {
            int index = GetResourceIndex(initialResource.res);

            if (index < 0)
            {
                Debug.LogWarning($"BaseSystem: No se encuentra el recurso {initialResource.res}.");
                continue;
            }

            int maximum = (maxResources != null && index < maxResources.Length) ? maxResources[index] : int.MaxValue;

            resources[index].cantidad = Mathf.Clamp(initialResource.cantidad, 0, maximum);
        }
    }

    private int GetResourceIndex(Resource resource)
    {
        if (resources == null)
            return -1;

        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i].res == resource)
                return i;
        }

        return -1;
    }

    private void Update()
    {
        if (IsDestroyed())
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

        if (faction == Faction.ALLY)
        {
            for (int i = 0; i < towerSummons.Count; i++)
            {
                if (Utils.KeyPressed(towerSummons[i].key)) SpawnUnit(i);
            }
        }
    }


    public void SpawnUnit(int id = 0)
    {

        Debug.Log("Tipo de tropa : "+ id);
        spawnerMutex.WaitOne();

        try
        {
            if (id < 0 || id >= towerSummons.Count)
            {
                Debug.LogWarning("ID fuera de los límites de la lista.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogWarning("No hay un prefab asignado.");
                return;
            }

            TroopDefinition definition = towerSummons[id].towerInfo;

            if (definition == null)
            {
                Debug.LogWarning(
                    $"No hay una definición asignada para la tropa {id}."
                );

                return;
            }

            if (troopPoolManager == null)
            {
                Debug.LogError(
                    "No se ha encontrado TroopPoolManager."
                );

                return;
            }

            // Provisional: Las tropas aliadas necesitan al menos un fungus, luego haremos que compruebe el numero de recursos necesarios para generarla.
            if (faction == Faction.ALLY && GetLeaves() <= 0)
            {
                Debug.LogWarning($"No se puede crear {definition.Name}: " + "no hay suficientes hongos.");
                return;
            }

            definition.FactionColor = (faction == Faction.ALLY) ? Color.white : Color.red;
            
            ATroop troop = troopPoolManager.Spawn(definition, faction, transform.position);

            if (troop == null)
            {
                Debug.LogWarning("No se ha podido invocar la tropa. Comprueba que el pool tenga objetos disponibles.");

                return;
            }

            if (faction == Faction.ALLY)
            {
                resources[0].cantidad--;

                UpdateObservers(new GameEvent<ResourceSlot>(LogicEvent.RESOURCE_GATHER_1 ,new ResourceSlot(Resource.LEAVES,resources[0].cantidad)));

                Debug.Log($"Tropa creada: {troop.GetDefinition().Name}. Hongos restantes: {resources[0].cantidad}");
            }
        }
        finally
        {
            spawnerMutex.ReleaseMutex();
        }
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            SpawnUnit();
        }
    }

    #region PATRÓN OBSERVER
    public void UpdateObserver(GameEvent<int> data)
    {
        bool _cond = (data.logicEvent == LogicEvent.TROOP_PLACED && faction == Faction.ALLY);
        Debug.Log("Spawneo?" + _cond);
        if (data.logicEvent == LogicEvent.TROOP_PLACED && faction == Faction.ALLY)
        {
            SpawnUnit(data.data);
        }
    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        Debug.Log("Info de recurso recibida!");
        
        if ((data.logicEvent == LogicEvent.RESOURCE_GATHER_1 || data.logicEvent == LogicEvent.RESOURCE_GATHER_2) && faction == Faction.ALLY)
        {
            int pos = GetResourceIndex(data.data.res);
            resources[pos].cantidad = Utils.Clamp(++resources[pos].cantidad, 0, maxResources[pos]);
            UpdateObservers(new GameEvent<ResourceSlot>(data.logicEvent, new ResourceSlot(data.data.res, resources[pos].cantidad)));
            Debug.Log($"Ahora tengo {resources[pos].cantidad} del recurso {resources[pos].res}");
        }
    }
    #endregion
}