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

public class BaseSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Faction faction;
    private HealthComponent health;

    [Header("Lógica de Spawner")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<TowerSummon> towerSummons;
    private Mutex spawnerMutex = new Mutex();


    private void Start()
    {
        health = this.gameObject.GetComponent<HealthComponent>();

        if(faction == Faction.ENEMY)
            StartCoroutine(SpawnCoroutine());
    }

    private void Update()
    {
        for(int i = 0; i < towerSummons.Count; i++)
        {
            if (Utils.KeyPressed(towerSummons[i].key)) SpawnUnit(i);   
        }
    }

    public void SpawnUnit(int id = 0)
    {
        spawnerMutex.WaitOne();
        Debug.Log("Colocando torre...");

        if(id < 0 || id > towerSummons.Count)
        {
            Debug.LogWarning("ID fuera de los límites de la lista.");
            return;

        }
        else if (prefab == null)
        {
            Debug.LogWarning("No hay un prefab de torre asignado para colocar.");
            return;
        }

        Troop troop = Instantiate(prefab).GetComponent<Troop>();
        troop.Initialize(towerSummons[id].towerInfo);
        Debug.Log($"Torre colocada: { troop.GetDefinition().Name }");

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