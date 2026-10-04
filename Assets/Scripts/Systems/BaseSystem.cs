using UnityEngine;

public class BaseSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Faction faction;
    private HealthComponent health;
    private Resource[] resources;

    public BaseSystem(int maxHealth)
    {
        health = new HealthComponent(maxHealth);
    }

    public void SpawnUnit()
    {
        //
    }

    public bool CanSpawn()
    {
        return false;
    }

    public bool IsDestroyed() => health.IsDead();

    public void GetAvailableResources()
    {
        if(faction != Faction.ALLY) return;

        //
    }
}