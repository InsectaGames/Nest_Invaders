using System.Collections.Generic;
using UnityEngine;

public class TroopPoolManager : ASubject<GameEvent<ATroop>>
{
    [Header("Configuración del pool")]
    [SerializeField] private ATroop prefab;
    [SerializeField, Min(1)] private int initialSize = 50;

    private IObjectPool<ATroop> pool;

    // Registro de tropas actualmente activas.
    private readonly HashSet<ATroop> activeTroops = new();

    private void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError("TroopPoolManager: no se ha asignado el prefab de Troop.", this);
            enabled = false;
            return;
        }

        pool = new AObjectPool<ATroop>(prefab, initialSize, transform);
    }

    public int GetTroopCount(TroopType type, Faction faction)
    {
        int count = 0;

        foreach (ATroop troop in activeTroops)
        {
            if (troop == null || troop.GetDefinition() == null)
                continue;

            if (troop.GetDefinition().type == type && troop.GetFaction() == faction)
                count++;
        }

        return count;
    }

    public bool CanSpawn(TroopDefinition definition, Faction faction)
    {
        if (definition == null)
            return false;

        return GetTroopCount(definition.type, faction) < definition.MaxActiveTroops;
    }

    public ATroop Spawn(TroopDefinition definition,Faction faction, Vector3 position)
    {
        if (pool == null || definition == null)
        {
            Debug.LogError("No se puede invocar la tropa: el pool o la definición no son válidos.");
            return null;
        }

        // Comprobar el límite antes de solicitar una unidad al pool.
        if (!CanSpawn(definition, faction))
            return null;

        ATroop troop = pool.Get();

        // El pool puede quedarse sin objetos disponibles.
        if (troop == null)
            return null;

        // Se coloca el objeto mientras sigue inactivo, se activa y configura.
        troop.transform.position = position;
        troop.gameObject.SetActive(true);

        // No contabilizar unidades cuya inicialización haya fallado.
        if (!troop.Initialize(definition, faction, this))
        {
            pool.Release(troop);
            return null;
        }

        activeTroops.Add(troop);

        UpdateObservers(new GameEvent<ATroop>(LogicEvent.TROOP_PLACED, troop));

        return troop;
    }

    public void Despawn(ATroop troop)
    {
        if (pool == null || troop == null)
            return;

        // Evitar descontar o devolver dos veces la misma tropa.
        if (!activeTroops.Remove(troop))
            return;

        UpdateObservers(new GameEvent<ATroop>(LogicEvent.TROOP_DESPAWN, troop));
        
        pool.Release(troop);
    }
}