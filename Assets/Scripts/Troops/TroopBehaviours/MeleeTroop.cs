using UnityEngine;

public class MeleeTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    public TroopType Type => TroopType.MELEE_ATTACKER;

    [Header("Combate")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float enemyAttackPriorityDelay = 0.2f; // PRIORIDAD DE ATAQUE A ALIADOS.

    private float nextAttackTime = 0f;
    private float enemyStartTime;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

         // Comprobar si ya puede volver a atacar.
        if (Time.time < nextAttackTime)
            return;

        // Prioridad de ataque aliada, enemigos tardan más.
        if (troop.GetFaction() == Faction.ENEMY && Time.time < enemyStartTime + enemyAttackPriorityDelay)
            return;

        if (CombatSystem.CloseEnoughToAttack(troop, out ATroop closestEnemy))
        {
            Attack(closestEnemy);

            // Establecer el siguiente momento de ataque.
                nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (troop == null)
            return;

        enemyStartTime = Time.time;

        // Buscar la base enemiga y establecerla como destino.
        Faction enemyFaction = (troop.GetFaction() == Faction.ALLY)? Faction.ENEMY : Faction.ALLY;

        Transform enemyBase = troop.FindBaseTransform(enemyFaction);

        if (enemyBase != null)
        {
            troop.SetTarget(enemyBase);
        }
        else
        {
            Debug.LogWarning($"{name}: no se ha encontrado la base enemiga.");
        }
    }

    // Método provisional para atacar.
    private void Attack(ATroop target)
    {
        if (target == null)
            return;

        CombatSystem.PerformAttack(troop, target);

        HealthComponent targetHealth = target.GetHealth();

        if (targetHealth != null && targetHealth.IsDead())
        {
            // Si el enemigo suelta recursos, realizar la tirada.
            if (target.GetFaction() != Faction.ALLY)
                GambleForSticks(target.GetDefinition());

            targetHealth.Die();
        }
    }

    // Método para conseguir palos. De momento, simplemente será una tirada aleatoria en todos los enemigos.
    // En el futuro, solo se debe aplicar a los bichos que suelten palos.
    private void GambleForSticks(TroopDefinition def)
    {
        if (Utils.GetRandomInclusive(0, 100) > def.chanceToDropSticks)
        {
            troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_2, new ResourceSlot(Resource.STICK, 1));
        }
    }

    public void ResetBehaviour()
    {
        // No hay ningún estado adicional que restablecer por ahora.
        nextAttackTime = 0f;
    }
}