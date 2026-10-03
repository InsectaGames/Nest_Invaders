public class HealthComponent
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }

    public HealthComponent(int maxHealth = 100)
    {
        Health = maxHealth;
        MaxHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        int result = Health - amount;
        Health = (result < 0) ? 0 : result;
        if(IsDead()) { Die(); }
    }

    public void Heal(int amount)
    {
        int result = Health + amount;
        Health = (result > MaxHealth) ? MaxHealth : result;
    }

    public bool IsDead() => Health == 0;

    public void Die()
    {
        // Devolver al object pool.
    }
}