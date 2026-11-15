
using UnityEngine;
using UnityEngine.AI;

public class ThiefTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;
    private NavMeshAgent agent;

    public TroopType Type => TroopType.THIEF;

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, currentTarget);

    [Header("Detección")]
    [SerializeField, Min(1f)] private float detectionRadius = 8f;
    [SerializeField, Min(0.05f)] private float targetUpdateInterval = 0.25f;

    [Header("Combate")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    [Header("Merodeo")]
    [SerializeField, Min(1f)] private float wanderRadius = 5f;
    [SerializeField, Min(0.1f)] private float wanderInterval = 3f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    private Transform gatherPoint;
    private ATroop currentTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
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

        if (gatherPoint == null)
            FindGatherPoint();

        // Buscar objetivos periódicamente.
        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateInterval;

            ATroop bestTarget = FindBestTarget();

            currentTarget = (bestTarget != null) ? bestTarget : null;
        }

        if (IsInCombat)
        {
            HandleCombat();
        }
        else
        {
            currentTarget = null;
            HandleWandering();
        }
    }

    #region INITIALIZATION

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        currentTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextWanderTime = 0f;

        FindGatherPoint();

        if (troop != null)
            troop.SetMovementStopped(false);
    }

    public void ResetBehaviour()
    {
        currentTarget = null;
        gatherPoint = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextWanderTime = 0f;

        if (troop != null)
            troop.SetMovementStopped(false);
    }

    private void FindGatherPoint()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("GatherPoint");

        if (obj != null)
            gatherPoint = obj.transform;
    }

    #endregion

    #region TARGET DETECTION

    private ATroop FindBestTarget()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        ATroop closestGatherer = null;
        ATroop closestEnemy = null;

        float gathererDistance = Mathf.Infinity;
        float enemyDistance = Mathf.Infinity;

        foreach (Collider2D col in detectedObjects)
        {
            if (col == null)
                continue;

            ATroop candidate = col.GetComponentInParent<ATroop>();

            if (!CombatSystem.IsValidEnemy(troop, candidate))
                continue;

            float distance = (candidate.transform.position - transform.position).sqrMagnitude;

            if (candidate.GetDefinition().type == TroopType.GATHERER)
            {
                if (distance < gathererDistance)
                {
                    gathererDistance = distance;
                    closestGatherer = candidate;
                }
            }
            else if (distance < enemyDistance)
            {
                enemyDistance = distance;
                closestEnemy = candidate;
            }
        }

        // Los Gatherers siempre tienen prioridad.
        return closestGatherer != null ? closestGatherer : closestEnemy;
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
                AttackTarget();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            troop.SetMovementStopped(false);
            troop.SetTarget(currentTarget.transform);
        }
    }

    private void AttackTarget()
    {
        if (!IsInCombat)
        {
            currentTarget = null;
            return;
        }

        CombatSystem.PerformAttack(troop, currentTarget);

        HealthComponent targetHealth = currentTarget.GetHealth();

        if (targetHealth != null && targetHealth.IsDead())
        {
            targetHealth.Die();
            currentTarget = null;
            troop.SetMovementStopped(false);
        }
    }

    #endregion

    #region WANDERING

    private void HandleWandering()
    {
        if (gatherPoint == null)
            return;

        if (agent.pathPending)
            return;

        if (agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathInvalid && agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            return;
        }

        if (Time.time < nextWanderTime)
            return;

        nextWanderTime = Time.time + wanderInterval;

        SetRandomWanderPosition();
    }

    private void SetRandomWanderPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;

            Vector3 candidate = gatherPoint.position + new Vector3(offset.x, offset.y, 0f);

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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (gatherPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gatherPoint.position, wanderRadius);
        }
    }
}