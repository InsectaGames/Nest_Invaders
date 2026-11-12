using UnityEngine;
using UnityEngine.AI;

public class NightTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;

    public TroopType Type => TroopType.NIGHT_CREATURE;

    [Header("Apariencia")]
    [SerializeField] private Color nightTint = new Color(0.65f, 0.35f, 0.85f, 1f);
    private Color originalColor;

    [Header("Detección")]
    [SerializeField, Min(1f)] private float detectionRadius = 6f;

    [SerializeField, Min(0.05f)] private float targetUpdateInterval = 0.25f;

    [Header("Combate")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, currentTarget);

    [Header("Merodeo")]
    [SerializeField, Min(1f)] private float wanderRadius = 5f;

    [SerializeField, Min(0.1f)] private float wanderInterval = 3f;

    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    private ATroop currentTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
    private float nextWanderTime;

    private bool isAttacking;

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        UpdateSpriteVisuals();

        // Buscar o actualizar el objetivo periódicamente.
        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateInterval;

            if (!IsValidTarget(currentTarget))
                currentTarget = FindClosestTarget();
        }

        // Si hay un objetivo válido, perseguirlo o atacarlo.
        if (IsValidTarget(currentTarget))
        {
            HandleCombat();
        }
        else
        {
            currentTarget = null;
            isAttacking = false;

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

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        currentTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextWanderTime = 0f;

        isAttacking = false;

        if (spriteRenderer != null)
            spriteRenderer.color = nightTint;

        if (troop != null)
            troop.SetMovementStopped(false);
    }

    public void ResetBehaviour()
    {
        currentTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextWanderTime = 0f;

        isAttacking = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    #endregion

    #region TARGET DETECTION

    private ATroop FindClosestTarget()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        ATroop closestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider2D detectedCollider in detectedObjects)
        {
            if (detectedCollider == null)
                continue;

            ATroop candidate = detectedCollider.GetComponentInParent<ATroop>();

            if (!IsValidTarget(candidate))
                continue;

            float distance = (candidate.transform.position - transform.position).sqrMagnitude;

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestTarget = candidate;
            }
        }

        return closestTarget;
    }

    private bool IsValidTarget(ATroop target) => CombatSystem.IsValidEnemy(troop, target);

    #endregion

    #region COMBAT

    private void HandleCombat()
    {
        if (!IsInCombat)
        {
            currentTarget = null;
            isAttacking = false;
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
            isAttacking = false;

            troop.SetMovementStopped(false);
            troop.SetTarget(currentTarget.transform);
        }
    }

    private void AttackTarget()
    {
        if (!IsInCombat)
        {
            currentTarget = null;
            isAttacking = false;
            return;
        }

        if(!isAttacking)
            isAttacking = true;

        CombatSystem.PerformAttack(troop, currentTarget);

        HealthComponent targetHealth = currentTarget.GetHealth();

        if (targetHealth != null && targetHealth.IsDead())
        {
            targetHealth.Die();

            currentTarget = null;
            isAttacking = false;

            troop.SetMovementStopped(false);
        }
    }

    #endregion

    #region WANDERING

    private void HandleWandering()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // Si ya tiene un destino válido, mantenerlo.
        if (agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathInvalid && agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            return;
        }

        if (Time.time < nextWanderTime)
            return;

        nextWanderTime = Time.time + wanderInterval;

        SetRandomWanderTarget();
    }

    private void SetRandomWanderTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;

            Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, agent.areaMask))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);

                return;
            }
        }
    }

    #endregion

    #region VISUALS

    private void UpdateSpriteVisuals()
    {
        if (spriteRenderer == null || agent == null)
            return;

        if (Mathf.Abs(agent.velocity.x) > 0.05f)
            spriteRenderer.flipX = agent.velocity.x < 0f;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}