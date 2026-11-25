using UnityEngine;

public class CombatSystem
{
    private static ATroop cachedSpringMiniBoss;

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

        if (target.GetDefinition().type == TroopType.GATHERER)
        {
            GathererTroop gathererTroop = target.GetComponent<GathererTroop>();

            if (gathererTroop == null || gathererTroop.GetState() == GathererState.Gathering)
                return false;
        }

        HealthComponent health = target.GetHealth();

        return health != null && !health.IsDead();
    }

    // Comprobar si el objetivo está dentro del alcance de ataque.
    public static bool CloseEnoughToAttack(ATroop attacker, ATroop target)
    {
        if (!IsValidEnemy(attacker, target))
            return false;

        float attackReach = attacker.GetDefinition().attackReach;
        float attackReachSqr = attackReach * attackReach;

        float sqrDistance = (target.transform.position - attacker.transform.position).sqrMagnitude;

        if (attacker.GetDefinition().type == TroopType.RANGED_ATTACKER)
        {
            if (target.GetDefinition().type == TroopType.SPRING_MINIBOSS)
            {
                return sqrDistance <= 0.3 * attackReachSqr;
            }
        }

        return sqrDistance <= attackReachSqr;
    }

    // Buscar la tropa enemiga más cercana dentro del radio de detección.
    public static ATroop FindClosestEnemy(ATroop attacker, float detectionRadius)
    {
        if (attacker == null || attacker.GetDefinition() == null)
            return null;

        // El minijefe tiene prioridad absoluta mientras siga vivo.
        ATroop boss = GetLivingSpringMiniBoss(attacker.GetFaction() == Faction.ALLY ? Faction.ENEMY : Faction.ALLY);

        if (boss != null && IsValidEnemy(attacker, boss))
            return boss;

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

    #region MINI JEFE

    public static ATroop GetLivingSpringMiniBoss(Faction faction)
    {
        if (cachedSpringMiniBoss != null)
        {
            HealthComponent health = cachedSpringMiniBoss.GetHealth();

            if (IsValidMiniBoss(cachedSpringMiniBoss, faction, health))
            {
                return cachedSpringMiniBoss;
            }

            cachedSpringMiniBoss = null;
        }

        ATroop[] troops = Object.FindObjectsByType<ATroop>(FindObjectsSortMode.None);

        foreach (ATroop candidate in troops)
        {
            if (candidate == null || candidate.GetDefinition() == null)
                continue;

            if (candidate.GetDefinition().type != TroopType.SPRING_MINIBOSS || candidate.GetFaction() != faction)
                continue;

            HealthComponent health = candidate.GetHealth();

            if (health == null || health.IsDead())
                continue;

            cachedSpringMiniBoss = candidate;
            return cachedSpringMiniBoss;
        }

        return null;
    }

    private static bool IsValidMiniBoss(ATroop miniBoss, Faction faction, HealthComponent health) =>
        miniBoss.gameObject.activeInHierarchy && miniBoss.GetDefinition() != null &&
        miniBoss.GetDefinition().type == TroopType.SPRING_MINIBOSS &&
        miniBoss.GetFaction() == faction && health != null && !health.IsDead();

    public static bool HasLivingSpringMiniBoss(Faction faction) => GetLivingSpringMiniBoss(faction) != null;

    #endregion
}