using System.Collections;
using UnityEngine;
using NavMeshPlus.Components;

public class NavMeshUpdater : MonoBehaviour 
{
    [Header("Obstacle Generation")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Sprite[] obstacleSprites;
    [SerializeField] private GameObject[] pointsOfInterest;

    [Header("Navigation")]
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private GathererPathFinder gathererPathfinder;

    private IEnumerator Start()
    {
        // Obtener el NavMeshSurface.
        if (surface == null)
            surface = GetComponent<NavMeshSurface>();

        if (gathererPathfinder == null)
            gathererPathfinder = GetComponent<GathererPathFinder>();

        LevelDefinition levelDefinition = GameTestMode.IsBehaviourTestScene ? null : LevelSession.SelectedLevel;

        Debug.Log("Nivel actual" + levelDefinition);

        int obstacleNumber = (levelDefinition != null) ? levelDefinition.obstacles.Length : 0;

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
  
        // Generar los obstáculos indicados por el nivel.
        for (int i = 0; i < obstacleNumber; i++)
        {
            GameObject instance = Instantiate(obstaclePrefab, (Vector3)levelDefinition.obstacles[i].pos , Quaternion.identity);
 
            SpriteRenderer spr = instance.GetComponent<SpriteRenderer>();

            spr.sprite = obstacleSprites[(int)levelDefinition.obstacles[i].obsType];

            AdjustCollider(spr , instance.GetComponent<CircleCollider2D>());        
        }

        Physics2D.SyncTransforms();
        yield return null;

        gathererPathfinder.BuildGrid();
        surface.BuildNavMesh();
    }

    private void AdjustCollider(SpriteRenderer spr , CircleCollider2D col)
    {
        if (spr == null || spr.sprite == null || col == null)
            return;

        col.radius = Utils.Max(spr.sprite.bounds.extents.x, spr.sprite.bounds.extents.y);
    } 
}