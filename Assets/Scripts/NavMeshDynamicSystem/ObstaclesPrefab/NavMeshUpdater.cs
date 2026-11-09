using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class NavMeshUpdater : MonoBehaviour 
{
    [Header("Obstacle Generation")]
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject[] pointsOfInterest;

    [Header("Navigation")]
    [SerializeField] NavMeshSurface surface;

    [Header("Configuración independiente de nivel")]
    [SerializeField] private int standaloneObstacleNumber = 0;

    private IEnumerator Start()
    {
        // Obtener el NavMeshSurface.
        if (surface == null)
            surface = GetComponent<NavMeshSurface>();

        LevelDefinition levelDefinition = GameTestMode.IsBehaviourTestScene ? null : LevelSession.SelectedLevel;
        int obstacleNumber = levelDefinition != null ? levelDefinition.obstacleNumber : standaloneObstacleNumber;

        if (levelDefinition == null && !GameTestMode.IsBehaviourTestScene)
        {
            Debug.LogError("NavMeshUpdater: No hay ningún nivel seleccionado en LevelSession.", this);
            yield break;
        }

        if (obstacleNumber > 0 && obstaclePrefab == null || surface == null)
        {
            Debug.LogError("Falta asignar obstaclePrefab o NavMeshSurface.", this);
            yield break;
        }

        // Obtener los puntos de interés de la escena y añadir las bases del nivel seleccionado.
        GameObject[] pois = GetPointsOfInterest();

        // Generar los obstáculos indicados por el nivel.
        for (int i = 0; i < obstacleNumber; i++)
        {
            GameObject instance = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);

            ObstacleScript obstacle = instance.GetComponent<ObstacleScript>();

            if (obstacle == null)
            {
                Debug.LogError("El prefab no tiene ObstacleScript.", instance);
                Destroy(instance);
                continue;
            }

            if (!obstacle.InitializePos(new Vector2(-16.99f, -9.28f), new Vector2(15.96f, 7.86f), pois))
            {
                Debug.LogWarning("No se encontró una posición válida para el obstáculo.");
                Destroy(instance);
            }
        }

        Physics2D.SyncTransforms();
        yield return null;
        surface.BuildNavMesh();
    }

    private GameObject[] GetPointsOfInterest()
    {
        List<GameObject> pois = new List<GameObject>();

        // Conservar los puntos asignados manualmente.
        if (pointsOfInterest != null)
        {
            foreach (GameObject poi in pointsOfInterest)
            {
                if (poi != null && !pois.Contains(poi))
                    pois.Add(poi);
            }
        }

        // Añadir las bases colocadas directamente en la escena.
        if(GameTestMode.IsBehaviourTestScene)
        {
            GameObject[] sceneBases = GameObject.FindGameObjectsWithTag("Base");

            foreach (GameObject baseObject in sceneBases)
            {
                if (baseObject != null && !pois.Contains(baseObject))
                    pois.Add(baseObject);
            }
        }
        else
        {
            // Añadir las bases instanciadas para este nivel.
            LevelGameplayInitializer initializer = FindFirstObjectByType<LevelGameplayInitializer>();

            if (initializer != null)
            {
                AddPointOfInterest(pois, initializer.AllyBaseInstance);
                AddPointOfInterest(pois, initializer.EnemyBaseInstance);
            }
            else
            {
                Debug.LogWarning("NavMeshUpdater: No se ha encontrado LevelGameplayInitializer.");
            }
        }

        return pois.ToArray();
    }

    private void AddPointOfInterest(List<GameObject> pois, GameObject point)
    {
        if (point != null && !pois.Contains(point))
            pois.Add(point);
    }
}