
using UnityEngine;
using UnityEngine.AI;

public class TankTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;
    private NavMeshAgent agent;

    public TroopType Type => TroopType.TANK;

    [Header("Protección de aliados")]
    [SerializeField, Min(1f)] private float allyProtectionRadius = 8f;

    [Header("Defensa de la base")]
    [SerializeField, Min(1f)] private float defenseRadius = 10f;
    [SerializeField, Min(1f)] private float guardWanderRadius = 4f;

    [Header("Detección y combate")]
    [SerializeField, Min(1f)] private float enemyDetectionRadius = 8f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;
    [SerializeField, Min(0.05f)] private float targetUpdateInterval = 0.25f;

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, currentTarget);

    [Header("Movimiento")]
    [SerializeField, Min(0.1f)] private float wanderInterval = 3f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    private ATroop protectedAlly;
    private ATroop currentTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
    private float nextMovementUpdateTime;
    private float nextWanderTime;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateInterval;
            UpdateTargets();
        }

        if (IsInCombat)
        {
            HandleCombat();
        }
        else if (protectedAlly != null && protectedAlly.gameObject.activeInHierarchy)
        {
            currentTarget = null;
            MoveTowards(protectedAlly.transform);
        }
        else
        {
            currentTarget = null;
            HandleGuarding();
        }
    }

    #region INITIALIZATION

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        protectedAlly = null;
        currentTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextMovementUpdateTime = 0f;
        nextWanderTime = 0f;

        troop.SetMovementStopped(false);
    }

    public void ResetBehaviour()
    {
        protectedAlly = null;
        currentTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextMovementUpdateTime = 0f;
        nextWanderTime = 0f;

        if (troop != null)
            troop.SetMovementStopped(false);
    }

    #endregion

    #region TARGET DETECTION

    private void UpdateTargets()
    {
        protectedAlly = FindAllyInCombat();

        if (protectedAlly != null)
        {
            currentTarget = FindClosestEnemyAround(protectedAlly.transform.position, enemyDetectionRadius);
            return;
        }

        currentTarget = null;

        Transform allyBase = troop.GetAllyBaseTransform();

        if (allyBase != null)
        {
            currentTarget = FindClosestEnemyAround(allyBase.position, defenseRadius);
        }
    }

    private ATroop FindAllyInCombat()
    {
        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, allyProtectionRadius);

        ATroop closestAlly = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop candidate = col.GetComponentInParent<ATroop>();

            if (candidate == null || candidate == troop)
                continue;

            if (candidate.GetFaction() != troop.GetFaction())
                continue;

            ICombatState combatState = candidate.GetComponent<ICombatState>();

            if (combatState == null || !combatState.IsInCombat)
                continue;

            float distance = (candidate.transform.position - transform.position).sqrMagnitude;

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestAlly = candidate;
            }
        }

        return closestAlly;
    }

    private ATroop FindClosestEnemyAround(Vector3 position, float radius)
    {
        Collider2D[] detected = Physics2D.OverlapCircleAll(position, radius);

        ATroop closestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop candidate = col.GetComponentInParent<ATroop>();

            if (!CombatSystem.IsValidEnemy(troop, candidate))
                continue;

            float distance = (candidate.transform.position - position).sqrMagnitude;

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = candidate;
            }
        }

        return closestEnemy;
    }

    #endregion

    #region COMBAT

    private void HandleCombat()
    {
        if (!IsInCombat)
        {
            currentTarget = null;
            troop.SetMovementStopped(false);
            return;
        }

        if (CombatSystem.CloseEnoughToAttack(troop, currentTarget))
        {
            troop.SetMovementStopped(true);

            if (Time.time >= nextAttackTime)
            {
                CombatSystem.PerformAttack(troop, currentTarget);
                nextAttackTime = Time.time + attackCooldown;

                HealthComponent targetHealth = currentTarget.GetHealth();

                if (targetHealth != null && targetHealth.IsDead())
                {
                    targetHealth.Die();
                    currentTarget = null;
                    troop.SetMovementStopped(false);
                }
            }
        }
        else
        {
            MoveTowards(currentTarget.transform);
        }
    }

    #endregion

    #region MOVEMENT

    private void MoveTowards(Transform target)
    {
        if (target == null || Time.time < nextMovementUpdateTime)
            return;

        nextMovementUpdateTime = Time.time + targetUpdateInterval;

        troop.SetMovementStopped(false);
        troop.SetTarget(target);
    }

    private void HandleGuarding()
    {
        if (agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathInvalid && agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            return;
        }

        if (Time.time < nextWanderTime)
            return;

        nextWanderTime = Time.time + wanderInterval;

        SetRandomGuardPosition();
    }

    private void SetRandomGuardPosition()
    {
        Transform allyBase = troop.GetAllyBaseTransform();

        if (allyBase == null)
            return;

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * guardWanderRadius;

            Vector3 candidate = allyBase.position +
                new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, agent.areaMask))
            {
                troop.SetMovementStopped(false);
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, allyProtectionRadius);

        Gizmos.color = Color.red;

        if (troop != null && troop.GetAllyBaseTransform() != null)
        {
            Gizmos.DrawWireSphere(troop.GetAllyBaseTransform().position, defenseRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(troop.GetAllyBaseTransform().position, guardWanderRadius);
        }
    }
}
