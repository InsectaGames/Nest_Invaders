public class BaseSystem
{
    private HealthComponent health;
    // private Faction faction;
    // private Resource[] resources;

    public BaseSystem(int maxHealth)
    {
        health = new HealthComponent(maxHealth);
    }

    public void SpawnUnit()
    {
        
    }

    public bool CanSpawn()
    {
        return false;
    }

    public bool IsDestroyed() => health.IsDead();

    public void GetAvailableResources()
    {
        
    }
}