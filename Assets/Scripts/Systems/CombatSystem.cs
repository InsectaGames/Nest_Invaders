using UnityEngine;

public class CombatSystem
{
    #region ATAQUE

    // Por ahora a lo sencillo: Ataque de daño aleatorio.
    public static void PerformAttack(ATroop attacker, ATroop target)
    {
        if (!IsValidEnemy(attacker, target))
            return;

        ApplyDamage(target.gameObject, attacker.GetDefinition().strength);
    }
    
    // Aplicar el daño.
    private static void ApplyDamage(GameObject obj, int damage = 0)
    {
        HealthComponent health = obj.GetComponent<HealthComponent>();

        if (health == null || health.IsDead())
            return;

        health.TakeDamage(damage);
    }

    #endregion

    #region ALCANCE

    // Comprobar si una tropa es un enemigo válido.
    public static bool IsValidEnemy(ATroop attacker, ATroop target)
    {
        if (attacker == null || target == null || attacker == target)
            return false;

        if (attacker.GetDefinition() == null || target.GetDefinition() == null)
            return false;

        if (attacker.GetFaction() == target.GetFaction())
            return false;

        HealthComponent health = target.GetHealth();

        return health != null && !health.IsDead();
    }

    // Comprobar si el objetivo está dentro del alcance de ataque.
    public static bool CloseEnoughToAttack(ATroop attacker, ATroop target)
    {
        if (!IsValidEnemy(attacker, target))
            return false;

        float attackReach = attacker.GetDefinition().attackReach;

        float sqrDistance = (target.transform.position - attacker.transform.position).sqrMagnitude;

        return sqrDistance <= attackReach * attackReach;
    }

    // Buscar la tropa enemiga más cercana dentro del radio de detección.
    public static ATroop FindClosestEnemy(ATroop attacker, float detectionRadius)
    {
        if (attacker == null || attacker.GetDefinition() == null)
            return null;

        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attacker.transform.position, detectionRadius);

        ATroop closestEnemy = null;
        ATroop closestTank = null;

        float shortestDistance = Mathf.Infinity;
        float shortestTankDistance = Mathf.Infinity;

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null)
                continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();

            if (!IsValidEnemy(attacker, otherTroop))
                continue;

            float distance = (otherTroop.transform.position - attacker.transform.position).sqrMagnitude;

            if (otherTroop.GetDefinition().type == TroopType.TANK)
            {
                if (distance < shortestTankDistance)
                {
                    shortestTankDistance = distance;
                    closestTank = otherTroop;
                }
            }
            else if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = otherTroop;
            }
        }

        return closestTank != null ? closestTank : closestEnemy;
    }

    #endregion
}