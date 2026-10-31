using UnityEngine;

public class CombatSystem
{
    // Daño provisional para las tropas melee.
    private const int meleeDamage = 9999;

    // Por ahora a lo sencillo: Ataque de daño aleatorio.
    public static void PerformAttack(ATroop attacker, ATroop target)
    {
        if (attacker == null || target == null)
            return;

        if (attacker.GetFaction() == target.GetFaction())
            return;

        HealthComponent health = target.GetHealth();

        if (health == null || health.IsDead())
            return;

        health.TakeDamage(meleeDamage);
    }
    
    // Aplicar el daño.
    private static void ApplyDamage(GameObject obj, int damage = 0)
    {
        HealthComponent health = obj.GetComponent<HealthComponent>();

        if (health == null || health.IsDead())
            return;

        health.TakeDamage(damage);
    }

    // Comprobar si está lo suficientemente cerca para atacar, y devolver el enemigo más cercano.
    public static bool CloseEnoughToAttack(ATroop obj, out ATroop closestEnemy)
    {
        closestEnemy = null;

        if (obj == null || obj.GetDefinition() == null)
            return false;

        float attackReach = obj.GetDefinition().attackReach;

        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(obj.transform.position, attackReach);

        float shortestDistance = Mathf.Infinity;
        Vector2 currentPosition = obj.transform.position;

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null)
                continue;

            // Buscar ATroop también en los padres del collider.
            ATroop otherTroop = collider.GetComponentInParent<ATroop>();

            if (otherTroop == null || otherTroop == obj || otherTroop.GetDefinition() == null || otherTroop.GetFaction() == obj.GetFaction())
                continue;

            // Ignorar enemigos que ya estén muertos.
            HealthComponent enemyHealth = otherTroop.GetHealth();

            if (enemyHealth == null || enemyHealth.IsDead())
                continue;

            float distance = (otherTroop.transform.position - (Vector3) currentPosition).sqrMagnitude;

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = otherTroop;
            }
        }

        return closestEnemy != null && shortestDistance <= attackReach * attackReach;
    }
}