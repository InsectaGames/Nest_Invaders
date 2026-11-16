using UnityEngine;

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
}