using UnityEngine;
using UnityEngine.AI;

public class LevelGameplayInitializer : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform allyBaseSpawnPoint;
    [SerializeField] private Transform enemyBaseSpawnPoint;
    [SerializeField] private Transform resourceAreaSpawnPoint;

    public GameObject AllyBaseInstance { get; private set; }
    public GameObject EnemyBaseInstance { get; private set; }
    public GameObject ResourceAreaInstance { get; private set; }

    private void Awake()
    {
        if (GameTestMode.IsBehaviourTestScene)
        {
            Debug.Log("Arena de pruebas: las bases se configuran manualmente desde el Editor.");
            return;
        }

        LevelDefinition level = LevelSession.SelectedLevel;

        if (level == null)
        {
            Debug.LogError("LevelGameplayInitializer: No hay ningún nivel seleccionado.", this);
            return;
        }

        Vector2 allyPosition = GetRandomPosition(level.allyBasePositions, allyBaseSpawnPoint);
        Vector2 enemyPosition = GetRandomPosition(level.enemyBasePositions, enemyBaseSpawnPoint);
        Vector2 resourceAreaPosition = GetRandomPosition(level.resourceAreaPositions, resourceAreaSpawnPoint);

        if (level.IsMidBossStage)
        {
            SpawnMidBoss(level, enemyPosition, allyPosition);
        }

        AllyBaseInstance = SpawnBase(level.allyBase, allyPosition, "aliada");
        EnemyBaseInstance = SpawnBase(level.enemyBase, enemyPosition, "enemiga");
        ResourceAreaInstance = SpawnBase(level.resourceArea, resourceAreaPosition, "recursos");

        Debug.Log($"Gameplay inicializado para: {level.Name}");
    }

    private Vector2 GetRandomPosition(Vector2[] positions, Transform fallback)
    {
        if (positions != null && positions.Length > 0)
        {
            return positions[Utils.GetRandom(0, positions.Length)];
        }

        return fallback != null ? (Vector2) fallback.position : Vector2.zero;
    }

    private GameObject SpawnBase(GameObject prefab, Vector2 position, string baseName)
    {
        if (prefab == null)
        {
            Debug.LogError($"No se ha asignado el prefab de la base {baseName}.", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);

        return instance;
    }

    private void SpawnMidBoss(LevelDefinition level, Vector2 enemyPosition, Vector2 allyPosition)
    {
        if (level.bossPrefab == null)
        {
            Debug.LogError($"El nivel {level.Name} tiene IsMidBossStage activo, pero no tiene bossPrefab asignado.", this);
            return;
        }

        // Dirección desde la base enemiga hacia la base aliada.
        Vector2 direction = (allyPosition - enemyPosition).normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.left;

        Vector3 desiredPosition = (Vector3) enemyPosition + (Vector3)(direction * LevelSession.SelectedLevel.midBossSpawnDistance);

        // El jefe necesita aparecer sobre el NavMesh.
        if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogError("No se ha encontrado una posición válida en el NavMesh para el mini jefe.", this);
            return;
        }

        GameObject bossObject = Instantiate(level.bossPrefab, hit.position, Quaternion.identity);

        ATroop bossTroop = bossObject.GetComponent<ATroop>();

        if (bossTroop == null)
        {
            Debug.LogError("El prefab del mini jefe necesita un componente ATroop en su objeto raíz.", bossObject);
            Destroy(bossObject);
            return;
        }

        TroopDefinition definition = bossTroop.GetDefinition();

        if (definition == null || definition.type != TroopType.SPRING_MINIBOSS)
        {
            Debug.LogError("El prefab del mini jefe necesita una TroopDefinition de tipo SPRING_MINIBOSS.", bossObject);
            Destroy(bossObject);
            return;
        }

        // Inicialización explícita como enemigo.
        bossTroop.Initialize(definition, Faction.ENEMY, null);
    }
}