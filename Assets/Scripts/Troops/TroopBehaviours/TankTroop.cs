using UnityEngine;
using UnityEngine.AI;

public class TankTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;
    private NavMeshAgent agent;

    public TroopType Type => TroopType.TANK;

    [Header("Protección de aliados")]
    [SerializeField, Min(1f)] private float allyProtectionRadius = 8f;

    [Header("Defensa de la mina")]
    [SerializeField, Min(1f)] private float defenseRadius = 5f;
    [SerializeField, Min(1f)] private float guardWanderRadius = 4f;
    private Transform gatherPoint;

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

    private Animator animator;

    private bool isAttacking;
    private ATroop attackTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
    private float nextMovementUpdateTime;
    private float nextWanderTime;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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

        // No interrumpir una animación de ataque en curso.
        if (isAttacking)
            return;

        if (IsInCombat)
        {
            HandleCombat();
            return;
        }
        
        // Si el objetivo ha desaparecido o ha muerto, limpiar el estado.
        currentTarget = null;
        troop.SetMovementStopped(false);

        // Continuar hacia el GatherPoint o patrullar alrededor de él.
        HandleGuarding();
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

        FindGatherPoint();

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextMovementUpdateTime = 0f;
        nextWanderTime = 0f;

        isAttacking = false;
        attackTarget = null;

        if (animator != null)
            animator.SetBool("tank_attacking", false);

        troop.SetMovementStopped(false);
    }

    public void ResetBehaviour()
    {
        protectedAlly = null;
        currentTarget = null;

        if(gatherPoint != null)
            gatherPoint = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextMovementUpdateTime = 0f;
        nextWanderTime = 0f;

        isAttacking = false;
        attackTarget = null;

        if (animator != null)
            animator.SetBool("tank_attacking", false);

        if (troop != null)
            troop.SetMovementStopped(false);
    }

    #endregion

    #region TARGET DETECTION

    private void UpdateTargets()
    {
        // Asegurar que tenemos localizado el punto de recolección.
        if (gatherPoint == null)
            FindGatherPoint();

        // Prioridad 1: Proteger aliados cercanos que estén combatiendo.
        protectedAlly = FindAllyInCombat();

        if (protectedAlly != null)
        {
            currentTarget = FindClosestEnemyAround(protectedAlly.transform.position, enemyDetectionRadius);

            if (currentTarget != null)
                return;
        }

        // Prioridad 2: Detectar enemigos alrededor del propio tanque. Esto permite interrumpir el desplazamiento hacia el GatherPoint.
        currentTarget = FindClosestEnemyAround(transform.position, enemyDetectionRadius);

        if (currentTarget != null)
            return;

        // Prioridad 3: Detectar enemigos que amenacen la zona de recolección.
        currentTarget = gatherPoint != null ? FindClosestEnemyAround(gatherPoint.position, defenseRadius) : null;
    }

    private void FindGatherPoint()
    {
        GameObject point = GameObject.FindGameObjectWithTag("GatherPoint");
        gatherPoint = point != null ? point.transform : null;
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

            if(candidate.GetDefinition().type == TroopType.SPRING_MINIBOSS)
            {
                continue;
            }
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
                Attack(currentTarget);
            }
        }
        else
        {
            animator.SetBool("tank_attacking", false);
            MoveTowards(currentTarget.transform);
        }
    }

    private void Attack(ATroop target)
    {
        if (isAttacking || !CombatSystem.IsValidEnemy(troop, target))
            return;

        isAttacking = true;
        attackTarget = target;

        nextAttackTime = Time.time + attackCooldown;

        troop.SetMovementStopped(true);
        animator.SetBool("tank_attacking", true);
    }

    public void ApplyTankAttackDamage()
    {
        if (!isAttacking || !CombatSystem.IsValidEnemy(troop, attackTarget))
            return;

        if (!CombatSystem.CloseEnoughToAttack(troop, attackTarget))
            return;

        CombatSystem.PerformAttack(troop, attackTarget);

        HealthComponent targetHealth = attackTarget.GetHealth();

        if (targetHealth != null && targetHealth.IsDead())
        {
            targetHealth.Die();
            currentTarget = null;
        }
    }

    public void FinishTankAttackAnimation()
    {
        isAttacking = false;
        attackTarget = null;

        if (animator != null)
            animator.SetBool("tank_attacking", false);

        if (!IsInCombat)
        {
            currentTarget = null;
            troop.SetMovementStopped(false);
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
        if (gatherPoint == null)
        return;

        // Primero debe llegar a la zona de recolección.
        float distanceToGatherPoint = Vector2.Distance(transform.position, gatherPoint.position);

        if (distanceToGatherPoint > guardWanderRadius)
        {
            MoveTowards(gatherPoint);
            return;
        }

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
        if (gatherPoint == null)
            return;

        float minRadius = guardWanderRadius * 0.4f; 

        for (int i = 0; i < 10; i++)
        {
            float angle = Utils.GetRandomFloat(0f, Mathf.PI);
            float distance = Utils.GetRandomFloat(minRadius, guardWanderRadius);

            Vector2 offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, allyProtectionRadius);

        if (gatherPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gatherPoint.position, defenseRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gatherPoint.position, guardWanderRadius);
        }
    }
}
