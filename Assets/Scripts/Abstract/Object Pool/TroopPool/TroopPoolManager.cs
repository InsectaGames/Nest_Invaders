using UnityEngine;

public class TroopPoolManager : MonoBehaviour
{
    [Header("Configuración del pool")]
    [SerializeField] private Troop prefab;
    [SerializeField, Min(1)] private int initialSize = 20;

    private IObjectPool<Troop> pool;

    private void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError("TroopPoolManager: no se ha asignado el prefab de Troop.", this);

            enabled = false;
            return;
        }

        pool = new AObjectPool<Troop>(prefab, initialSize, transform);
    }

    public Troop Spawn(TroopDefinition definition,Faction faction, Vector3 position)
    {
        if (pool == null || definition == null)
        {
            Debug.LogError(
                "No se puede invocar la tropa: el pool o la definición no son válidos."
            );

            return null;
        }

        Troop troop = pool.Get();

        // El pool puede quedarse sin objetos disponibles.
        if (troop == null)
            return null;

        // Primero colocamos el objeto mientras sigue inactivo.
        troop.transform.position = position;

        // Después lo activamos y configuramos.
        troop.gameObject.SetActive(true);
        troop.Initialize(definition, faction, this);

        return troop;
    }

    public void Despawn(Troop troop)
    {
        if (pool == null || troop == null)
            return;

        pool.Release(troop);
    }
}