using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using System.Collections;
using Unity.VisualScripting;

public class BaseSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Faction faction;
    private HealthComponent health;

    [Header("Lógica de Spawner")]
    [SerializeField] private GameObject towerPrefab;
    private Mutex spawnerMutex = new Mutex();
    
    [Tooltip("Tecla para activar el modo de colocación.")]
    [SerializeField] private Key placementKey = Key.T;

    private void Start()
    {
        health = this.gameObject.GetComponent<HealthComponent>();

        if(faction == Faction.ENEMY)
            StartCoroutine(SpawnCoroutine());
    }

    private void Update()
    {
        if (Utils.KeyPressed(placementKey)) SpawnUnit();
    }

    public void SpawnUnit()
    {
        spawnerMutex.WaitOne();
        Debug.Log("Colocando torre...");

        if (towerPrefab == null)
        {
            Debug.LogWarning("No hay un prefab de torre asignado para colocar.");
            return;
        }

        Instantiate(towerPrefab);
        Debug.Log("Torre colocada.");

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

    public bool IsDestroyed() => health.IsDead();
}