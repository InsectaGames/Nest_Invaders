using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
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

[System.Serializable]
public class DebugTroopEntry
{
    public TroopDefinition definition;
    public ATroop prefab;
}

public class BaseSystem : ASubject<GameEvent<ResourceSlot>>, IObserver<GameEvent<int>>, IObserver<GameEvent<ResourceSlot>>
{
    [Header("Configuración")]
    [SerializeField] private Faction faction;

    [Header("Configuración independiente de nivel (testing)")]
    [SerializeField] private int standaloneBaseHealth = 100;

    private HealthComponent health;

    [Header("Recursos")]
    [SerializeField] private ResourceSlot[] resources;
    [SerializeField] private int[] maxResources;

    [Header("Lógica de Spawner")]
    [SerializeField] private TroopPoolManager troopPoolManager;

    // Las invocaciones manuales son exclusivamente para aliados.
    [SerializeField] private List<TowerSummon> towerSummons;

    private Mutex spawnerMutex = new Mutex();

    // Configuración de generación enemiga.
    [SerializeField] private List<TroopDefinition> enemyTroops = new List<TroopDefinition>();

    private float enemySpawnInterval;
    private Coroutine enemySpawnCoroutine;

    private bool destructionHandled = false;

    [Header("Debug - Invocación de tropas")]
    [SerializeField] private bool enableDebugTroopSpawning = true;
    [SerializeField] private Key debugSpawnKey = Key.T;
    [SerializeField] private List<DebugTroopEntry> debugTroops = new List<DebugTroopEntry>();

    [SerializeField] private Transform debugSpawnPoint;
    [SerializeField] private float debugSpawnRadius = 2f;

    public Faction GetFaction() => faction;

    public int GetLeaves() => GetResourceAmount(Resource.LEAVES);
    public int GetSticks() => GetResourceAmount(Resource.STICK);

    private int GetResourceAmount(Resource resource)
    {
        int index = GetResourceIndex(resource);

        if (index < 0)
            return 0;

        return resources[index].cantidad;
    }

    public bool IsDestroyed() => health != null && health.IsDead();

    private void Awake()
    {
        if (faction == Faction.ALLY && !GameTestMode.IsBehaviourTestScene)
            ApplyInitialResources();
    }

    private void Start()
    {
        if (troopPoolManager == null)
        {
            troopPoolManager = FindFirstObjectByType<TroopPoolManager>();
        }

        health = GetComponent<HealthComponent>();

        LevelDefinition level = GameTestMode.IsBehaviourTestScene ? null : LevelSession.SelectedLevel;

        if (health != null)
        {
            int initialHealth = standaloneBaseHealth;

            if (level != null)
            {
                initialHealth = faction == Faction.ALLY ? level.allyBaseHealth : level.enemyBaseHealth;
            }

            health.Initialize(initialHealth);
        }
        else
        {
            Debug.LogError("BaseSystem: No se ha encontrado HealthComponent.", this);
        }

        switch (faction)
        {
            case Faction.ALLY:
                UITroopsGenerator generator = FindFirstObjectByType<UITroopsGenerator>();
                generator?.AddObserver(this);
            break;

            case Faction.ENEMY:
                if(level != null)
                    ConfigureEnemySpawner();
            break;
        }
    }

    #region RECURSOS

    private void ApplyInitialResources()
    {
        LevelDefinition level = LevelSession.SelectedLevel;

        if (level == null)
        {
            Debug.LogError("BaseSystem: No hay ningún nivel seleccionado.", this);
            return;
        }

        if (level.initialResources == null || resources == null)
        {
            Debug.LogWarning("BaseSystem: No se han configurado los recursos iniciales.", this);
            return;
        }

        foreach (ResourceSlot initialResource in level.initialResources)
        {
            int index = GetResourceIndex(initialResource.res);

            if (index < 0)
            {
                Debug.LogWarning($"No se encuentra el recurso {initialResource.res}.");
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

    #endregion

    #region GENERACIÓN DE ENEMIGOS

    private void ConfigureEnemySpawner()
    {
        LevelDefinition level = LevelSession.SelectedLevel;

        if (level == null)
        {
            Debug.LogError("BaseSystem: No hay ningún nivel seleccionado para generar enemigos.", this);
            return;
        }

        if (level.enemySpawnInterval <= 0f)
        {
            Debug.LogError("BaseSystem: El intervalo de aparición de enemigos debe ser mayor que cero.", this);
            return;
        }

        if (level.enemyTroops == null || level.enemyTroops.Length == 0)
        {
            Debug.LogWarning($"BaseSystem: El nivel {level.Name} no tiene tropas enemigas configuradas.", this);
            return;
        }

        enemySpawnInterval = level.enemySpawnInterval;

        enemyTroops.Clear();

        foreach (TroopDefinition troop in level.enemyTroops)
        {
            if (troop != null)
                enemyTroops.Add(troop);
        }

        if (enemyTroops.Count == 0)
        {
            Debug.LogWarning($"BaseSystem: La lista de enemigos de {level.Name} no contiene definiciones válidas.", this);
            return;
        }

        Debug.Log(
            $"Spawner enemigo configurado. " +
            $"Nivel: {level.Name} | " +
            $"Tropas disponibles: {enemyTroops.Count} | " +
            $"Intervalo: {enemySpawnInterval}s"
        );

        enemySpawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemySpawnInterval);

            if (IsDestroyed())
                yield break;

            SpawnEnemyUnit();
        }
    }

    private void SpawnEnemyUnit()
    {
        if (faction != Faction.ENEMY)
            return;

        if (enemyTroops.Count == 0)
            return;

        int randomIndex = Random.Range(0, enemyTroops.Count);

        TroopDefinition definition = enemyTroops[randomIndex];

        SpawnConfiguredTroop(definition, false);
    }

    private void StopEnemySpawner()
    {
        if (enemySpawnCoroutine != null)
        {
            StopCoroutine(enemySpawnCoroutine);
            enemySpawnCoroutine = null;
        }
    }

    #endregion

    #region DEBUG Y TESTING

    private ATroop SpawnConfiguredTroop(TroopDefinition definition, Vector3? spawnPosition = null)
    {
        if (definition == null)
        {
            Debug.LogWarning("No se puede invocar una tropa sin definición.", this);
            return null;
        }

        if (!GameTestMode.IsBehaviourTestScene)
        {
            Debug.LogWarning("SpawnConfiguredTroop está utilizando la implementación de debug fuera de la escena de pruebas.", this);
            return null;
        }

        // Buscar el prefab asociado a la definición.
        DebugTroopEntry entry = debugTroops.Find( troop => troop != null && troop.definition == definition);

        if (entry == null || entry.prefab == null)
        {
            Debug.LogWarning($"No existe un prefab configurado para la tropa {definition.name}.", this);
            return null;
        }

        Vector3 position = spawnPosition ?? transform.position;

        // Instanciar directamente el prefab, sin utilizar el pool.
        ATroop troop = Instantiate(entry.prefab, position, entry.prefab.transform.rotation);

        if (troop == null)
        {
            Debug.LogError($"No se pudo instanciar la tropa {definition.name}.", this);
            return null;
        }

        // Inicializar la tropa como unidad independiente.
        troop.Initialize(definition, faction, null);

        return troop;
    }

    private void SpawnAllDebugTroops()
    {
        if (faction != Faction.ENEMY)
            return;

        if (debugTroops == null || debugTroops.Count == 0)
        {
            Debug.LogWarning("No hay tropas configuradas en la lista de debug.", this);
            return;
        }

        Vector3 origin = debugSpawnPoint != null ? debugSpawnPoint.position : transform.position;

        int spawnedCount = 0;

        for (int i = 0; i < debugTroops.Count; i++)
        {
            DebugTroopEntry entry = debugTroops[i];

            if (entry == null || entry.definition == null || entry.prefab == null)
            {
                Debug.LogWarning($"La entrada {i} de la lista de debug está incompleta.", this);
                continue;
            }

            // Distribuir las tropas alrededor del punto de aparición.
            float angle = i * Mathf.PI * 2f / debugTroops.Count;

            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * debugSpawnRadius;

            Vector3 desiredPosition = origin + offset;

            // Buscar una posición válida sobre el NavMesh.
            if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"No se encontró una posición válida para {entry.definition.name}.", this);
                continue;
            }

            ATroop troop = SpawnConfiguredTroop(entry.definition, hit.position);

            if (troop != null)
                spawnedCount++;
        }

        Debug.Log($"Invocación de debug completada: {spawnedCount}/{debugTroops.Count} tropas creadas.", this);
    }

    #endregion

    #region GENERACIÓN DE TROPAS

    // Este método sigue siendo el punto de entrada para las invocaciones manuales de las tropas aliadas.
    public void SpawnUnit(int id = 0)
    {
        if (faction != Faction.ALLY)
            return;

        if (towerSummons == null || id < 0 || id >= towerSummons.Count)
        {
            Debug.LogWarning("ID fuera de los límites de la lista.");
            return;
        }

        TroopDefinition definition = towerSummons[id].towerInfo;

        if (definition == null)
        {
            Debug.LogWarning($"No hay una definición asignada para la tropa {id}.");
            return;
        }

        SpawnConfiguredTroop(definition, true);
    }

    // Método común para invocar tropas de cualquiera de las facciones.
    private ATroop SpawnConfiguredTroop(TroopDefinition definition, bool consumeLeaves)
    {
        spawnerMutex.WaitOne();

        try
        {
            if (definition == null)
            {
                Debug.LogWarning("La definición de tropa es nula.");
                return null;
            }

            if (troopPoolManager == null)
                troopPoolManager = FindFirstObjectByType<TroopPoolManager>();

            if (troopPoolManager == null)
            {
                Debug.LogError("No se ha encontrado TroopPoolManager.");
                return null;
            }

            int leavesIndex = -1;

            // Solo las tropas aliadas consumen recursos.
            if (consumeLeaves)
            {
                leavesIndex = GetResourceIndex(Resource.LEAVES);

                if (leavesIndex < 0 || resources[leavesIndex].cantidad <= 0)
                {
                    Debug.LogWarning($"No se puede crear {definition.Name}: no hay suficientes hojas.");
                    return null;
                }
            }

            ATroop troop = troopPoolManager.Spawn(definition, faction, transform.position);

            if (troop == null)
            {
                Debug.LogWarning("No se ha podido invocar la tropa. Comprueba que el pool tenga objetos disponibles.");
                return null;
            }

            if (consumeLeaves)
            {
                resources[leavesIndex].cantidad--;

                UpdateObservers(new GameEvent<ResourceSlot>(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.LEAVES, resources[leavesIndex].cantidad)));

                Debug.Log($"Tropa aliada creada: {definition.Name}. Hojas restantes: {resources[leavesIndex].cantidad}");
            }
            else
            {
                Debug.Log($"Tropa enemiga creada: {definition.Name}");
            }

            return troop;
        }
        finally
        {
            spawnerMutex.ReleaseMutex();
        }
    }

    #endregion

    #region UPDATE Y DESTRUCCIÓN

    private void Update()
    {
        // 1. La destrucción de la base debe comprobarse en TODAS las escenas.
        if (IsDestroyed() && !destructionHandled)
        {
            destructionHandled = true;

            StopEnemySpawner();

            // Ejecutar la muerte de la base.
            health.Die();

            GameObject controllerObject = GameObject.FindGameObjectWithTag("GameFlowController");

            GameFlowController gfc = controllerObject != null ? controllerObject.GetComponent<GameFlowController>() : null;

            if (gfc != null)
            {
                if (faction == Faction.ALLY)
                    gfc.TriggerDefeat();
                else if (faction == Faction.ENEMY)
                    gfc.TriggerVictory();
            }
            else
            {
                Debug.LogError("No se encontró GameFlowController en la escena.");
            }

            return;
        }

        // 2. Invocación múltiple de tropas: EXCLUSIVA de la escena de testing.
        if (GameTestMode.IsBehaviourTestScene && faction == Faction.ENEMY && enableDebugTroopSpawning && Utils.KeyPressed(debugSpawnKey))
        {
            SpawnAllDebugTroops();
        }

        // 3. Invocaciones manuales de la base aliada.
        // Se mantienen disponibles también durante las partidas normales.
        if (faction == Faction.ALLY && towerSummons != null)
        {
            for (int i = 0; i < towerSummons.Count; i++)
            {
                if (Utils.KeyPressed(towerSummons[i].key))
                    SpawnUnit(i);
            }
        }
    }

    #endregion

    #region PATRÓN OBSERVER

    public void UpdateObserver(GameEvent<int> data)
    {
        if (data.logicEvent == LogicEvent.TROOP_PLACED && faction == Faction.ALLY)
        {
            SpawnUnit(data.data);
        }
    }

    public void UpdateObserver(GameEvent<ResourceSlot> data)
    {
        if (faction != Faction.ALLY)
            return;

        if (data.logicEvent != LogicEvent.RESOURCE_GATHER_1 && data.logicEvent != LogicEvent.RESOURCE_GATHER_2)
            return;

        int index = GetResourceIndex(data.data.res);

        if (index < 0)
            return;

        int maximum = (maxResources != null && index < maxResources.Length) ? maxResources[index] : int.MaxValue;

        resources[index].cantidad = Utils.Clamp(resources[index].cantidad + data.data.cantidad, 0, maximum);

        UpdateObservers(new GameEvent<ResourceSlot>(data.logicEvent, new ResourceSlot(data.data.res, resources[index].cantidad)));

        Debug.Log($"Ahora tengo {resources[index].cantidad} del recurso " + resources[index].res);
    }

    #endregion
}