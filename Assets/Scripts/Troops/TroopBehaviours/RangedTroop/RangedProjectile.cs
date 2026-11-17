using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField, Min(0.1f)] private float speed = 10f;
    [SerializeField, Min(0.1f)] private float lifetime = 5f;

    private ATroop attacker;
    private ATroop target;
    private int damage;

    public void Initialize(ATroop attacker, ATroop target, int damage)
    {
        this.attacker = attacker;
        this.target = target;
        this.damage = damage;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!CombatSystem.IsValidEnemy(attacker, target))
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;
        float movement = speed * Time.deltaTime;

        if (distance <= movement)
        {
            HitTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, movement);

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void HitTarget()
    {
        if (CombatSystem.IsValidEnemy(attacker, target))
        {
            HealthComponent health = target.GetHealth();

            if (health != null)
            {
                health.TakeDamage(damage);

                if (health.IsDead())
                    health.Die();
            }
        }

        Destroy(gameObject);
    }
}