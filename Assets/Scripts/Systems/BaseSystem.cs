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

public struct TroopInstance
{
    public GameObject go;
    public int id;
}

public class BaseSystem : MonoBehaviour , IObserver<int>
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


        UITroopsGenerator generator =
            FindFirstObjectByType<UITroopsGenerator>();

        if (generator == null)
        {
            Debug.LogError("No se encontró UITroopsGenerator en la escena.");
            return;
        }

        generator.AddObserver(this);


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

  public Faction GetFaction() { return this.faction; }

    public bool IsDestroyed() => health.IsDead();

    public void UpdateObserver(int data)
    {
       if(faction == Faction.ALLY)
        {
            SpawnUnit(data);
            
        }
    }
}