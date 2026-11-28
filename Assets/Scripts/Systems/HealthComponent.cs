using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public GameObject obj { get; private set; }

    [SerializeField] private GameObject canv_health;

    [SerializeField] private Image bg_fill;
    [SerializeField] private Image fillbar;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite[] typeFill;
    [SerializeField] private Sprite[] typeBg;

    [SerializeField] private GameObject death_effect_ally;
    [SerializeField] private GameObject death_effect_enemy;
    
    public void Initialize( Faction f , int maxHealth = 100)
    {
        Health = maxHealth;
        MaxHealth = maxHealth;

        int index;

        switch (f)
        {
            case Faction.ALLY:
                index = 0;
                break;

            default:
                index = 1;
                break;
        }

        if (typeFill == null || typeFill.Length <= index || typeBg == null || typeBg.Length <= index)
        {
            Debug.LogError(
                $"Sprites insuficientes. Fill: {typeFill?.Length}, " +
                $"BG: {typeBg?.Length}, Índice solicitado: {index}"
            );
            return;
        }

        bg_fill.sprite = typeBg[index];
        fillbar.sprite = typeFill[index];

        spriteRenderer = this.GetComponent<SpriteRenderer>();

        canv_health.SetActive(false);

        UpdateHealthUI();
    }

    public void TakeDamage(int amount = 1)
    {
        int result = Health - amount;
        Health = (result < 0) ? 0 : result;

        if(this.gameObject.tag != "Base")
            StartCoroutine(RecieveDamageAnimation());
        
        UpdateHealthUI();
        // if(IsDead()) { Die(); }
    }
    private IEnumerator RecieveDamageAnimation()
    {
        spriteRenderer.color = Color.red;

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            spriteRenderer.color = Color.Lerp(Color.red, Color.white, elapsedTime / duration);

            yield return null;
        }

        spriteRenderer.color = Color.white;
        yield return null;
    }

    public void Heal(int amount = 1)
    {
        int result = Health + amount;
        Health = (result > MaxHealth) ? MaxHealth : result;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        canv_health.SetActive(Health < MaxHealth);
        float healthPercent = (float)Health / MaxHealth;

        // Actualizar relleno.
        fillbar.fillAmount = healthPercent;
       
        /*
        // Interpolar colores.
        if (healthPercent >= 0.5f)
        {
            // De amarillo a verde (100% - 50%).
            float t = (healthPercent - 0.5f) / 0.5f;
            fillbar.color = Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            // De rojo a amarillo (0% - 50%).
            float t = healthPercent / 0.5f;
            fillbar.color = Color.Lerp(Color.red, Color.yellow, t);
        }
        */

    }

    public bool IsDead() => Health == 0;

    public void Die()
    {
        ATroop troop = GetComponent<ATroop>();

        if (troop != null)
        {
            if (troop.GetFaction() == Faction.ALLY)
                Instantiate(death_effect_ally, transform.position, Quaternion.identity);
            else if (troop.GetFaction() == Faction.ENEMY)
                Instantiate(death_effect_enemy, transform.position, Quaternion.identity);

            AudioManager.Instance.UpdateObserver(new GameEvent<string>(LogicEvent.TROOP_DEATH, troop.GetFaction().ToString()));
            troop.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}