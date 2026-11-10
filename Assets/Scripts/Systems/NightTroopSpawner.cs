using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NightTroopSpawner : MonoBehaviour, IObserver<GameEvent<DayTime>>
{
    [Header("Referencias")]
    [SerializeField] private TroopDefinition nightTroopDefinition;
    private TroopPoolManager nightTroopPool;
    private DayNightSystem dayNightSystem;

    [Header("Zona de aparición")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField, Min(1f)] private float spawnRadius = 20f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    [Header("Frecuencia de aparición")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 8f;
    [SerializeField, Min(1)] private int maxNightTroops = 8;

    private readonly List<ATroop> spawnedTroops = new();

    private Coroutine spawnRoutine;
    private bool isNight;

    private void OnEnable()
    {
        dayNightSystem = FindFirstObjectByType<DayNightSystem>();
        nightTroopPool = FindFirstObjectByType<TroopPoolManager>();

        if (dayNightSystem == null)
        {
            Debug.LogError("NightTroopSpawner: No se encuentra DayNightSystem.");
            return;
        }

        dayNightSystem.AddObserver(this);

        if (nightTroopPool == null)
        {
            Debug.LogError("NightTroopSpawner: No se encuentra el pool de tropas.");
            return;
        }
    }

    private void OnDisable()
    {
        if (dayNightSystem != null)
            dayNightSystem.RemoveObserver(this);

        StopNightSpawning();

        dayNightSystem = null;
        nightTroopPool = null;
    }

    public void UpdateObserver(GameEvent<DayTime> gameEvent)
    {
        switch (gameEvent.data)
        {
            case DayTime.NIGHT:
                StartNightSpawning();
                break;

            case DayTime.DAY:
                StopNightSpawning();
                break;
        }
    }

    private void StartNightSpawning()
    {
        if (isNight)
            return;

        if (nightTroopPool == null || nightTroopDefinition == null || spawnCenter == null)
        {
            Debug.LogError("NightTroopSpawner: faltan referencias de configuración.", this);
            return;
        }

        isNight = true;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void StopNightSpawning()
    {
        isNight = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Devolver todas las criaturas al pool.
        for (int i = spawnedTroops.Count - 1; i >= 0; i--)
        {
            ATroop troop = spawnedTroops[i];

            if (troop != null)
                troop.ReturnToPool();
        }

        spawnedTroops.Clear();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isNight)
        {
            CleanSpawnedList();

            if (spawnedTroops.Count < maxNightTroops)
                TrySpawnNightTroop();

            yield return new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;
    }

    private void TrySpawnNightTroop()
    {
        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            return;

        ATroop troop = nightTroopPool.Spawn(nightTroopDefinition, Faction.NEUTRAL, spawnPosition);

        if (troop != null)
            spawnedTroops.Add(troop);
    }

    private bool TryGetSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;

        // Intentar varias posiciones por si alguna no es válida.
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;

            Vector3 candidate = spawnCenter.position + new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition( candidate, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
        }

        Debug.LogWarning("NightTroopSpawner: no se encontró una posición válida en el NavMesh.", this);

        return false;
    }

    private void CleanSpawnedList()
    {
        spawnedTroops.RemoveAll(troop => troop == null);
    }
}