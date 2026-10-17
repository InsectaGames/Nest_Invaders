
using System.Collections;
using UnityEngine;
using NavMeshPlus.Components;

public class NavMeshUpdater : MonoBehaviour 
{
    [Header("Level")]
    [SerializeField] LevelDefinition levelDefinition;

    [Header("Obstacle Generation")]
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject[] pointsOfInterest;

    [Header("Navigation")]
    [SerializeField] NavMeshSurface surface;



    IEnumerator Start()
    {
        // Obtener el NavMeshSurface
        if (surface == null)
            surface = GetComponent<NavMeshSurface>();

        if(levelDefinition == null)
        levelDefinition = GameObject.FindFirstObjectByType<DayNightSystem>().getLvlDef();

        if (levelDefinition == null ||
            obstaclePrefab == null ||
            surface == null)
        {
            Debug.LogError(
                "Falta asignar LevelDefinition, obstaclePrefab o NavMeshSurface.",
                this
            );
            yield break;
        }

        // Generar los obstáculos indicados por el nivel
        for (int i = 0; i < levelDefinition.obstacleNumber; i++)
        {
            GameObject instance = Instantiate(
                obstaclePrefab,
                Vector3.zero,
                Quaternion.identity
            );

            ObstacleScript obstacle = instance.GetComponent<ObstacleScript>();

            if (obstacle == null)
            {
                Debug.LogError("El prefab no tiene ObstacleScript.");
                Destroy(instance);
                continue;
            }

            if (!obstacle.InitializePos(new Vector2(-5.7f, -5.2f), new Vector2(12.4f, 7.3f), pointsOfInterest))
            {
                Debug.LogWarning("No se encontró una posición válida para el obstáculo.");
                Destroy(instance);
            }
        }

        Physics2D.SyncTransforms();
        yield return null;
        surface.BuildNavMesh();
    }
}
