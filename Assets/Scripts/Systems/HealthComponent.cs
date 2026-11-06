using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public GameObject obj { get; private set; }

    [SerializeField] private GameObject canv_health;
    [SerializeField] private Image fillbar;

    public void Initialize(int maxHealth = 100)
    {
        Health = maxHealth;
        MaxHealth = maxHealth;

        canv_health.SetActive(false);

        UpdateHealthUI();
    }

    public void TakeDamage(int amount = 1)
    {
        int result = Health - amount;
        Health = (result < 0) ? 0 : result;
     

        UpdateHealthUI();
        // if(IsDead()) { Die(); }
    }

    public void Heal(int amount = 1)
    {
        int result = Health + amount;
        Health = (result > MaxHealth) ? MaxHealth : result;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        float healthPercent = (float)Health / MaxHealth;

        // Actualizar relleno
        fillbar.fillAmount = healthPercent;

        // Interpolar colores
        if (healthPercent >= 0.5f)
        {
            // De amarillo a verde (100% - 50%)
            float t = (healthPercent - 0.5f) / 0.5f;
            fillbar.color = Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            // De rojo a amarillo (0% - 50%)
            float t = healthPercent / 0.5f;
            fillbar.color = Color.Lerp(Color.red, Color.yellow, t);
        }

    }

    public bool IsDead() => Health == 0;

    public void Die()
    {
        ATroop troop = GetComponent<ATroop>();

        if(troop != null)
        {
            troop.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}