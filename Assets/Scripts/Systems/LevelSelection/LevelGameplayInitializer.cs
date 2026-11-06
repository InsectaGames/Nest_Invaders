using UnityEngine;

public class LevelGameplayInitializer : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform allyBaseSpawnPoint;
    [SerializeField] private Transform enemyBaseSpawnPoint;

    public GameObject AllyBaseInstance { get; private set; }
    public GameObject EnemyBaseInstance { get; private set; }

    private void Awake()
    {
        LevelDefinition level = LevelSession.SelectedLevel;

        if (level == null)
        {
            Debug.LogError("LevelGameplayInitializer: No hay ningún nivel seleccionado.", this);
            return;
        }

        AllyBaseInstance = SpawnBase(level.allyBase, allyBaseSpawnPoint, "aliada");

        EnemyBaseInstance = SpawnBase(level.enemyBase, enemyBaseSpawnPoint, "enemiga");

        Debug.Log($"Gameplay inicializado para: {level.Name}");
    }

    private GameObject SpawnBase(GameObject prefab, Transform spawnPoint, string baseName)
    {
        if (prefab == null)
        {
            Debug.LogError($"No se ha asignado el prefab de la base {baseName}.", this);
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"No se ha asignado el punto de aparición de la base {baseName}.", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        return instance;
    }
}