
using UnityEngine;
using UnityEngine.AI;

public class RangedTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;
    private Animator animator;

    public TroopType Type => TroopType.RANGED_ATTACKER;

    [Header("Detección")]
    [SerializeField, Min(1f)] private float detectionRadius = 10f;
    [SerializeField, Min(0.1f)] private float dangerRadius = 3f;
    [SerializeField, Min(0.05f)] private float targetUpdateInterval = 0.2f;

    [Header("Combate")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, currentTarget);

    [Header("Proyectiles")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private bool isShooting;
    private ATroop shootingTarget;

    [Header("Reposicionamiento evasivo")]
    [SerializeField, Min(1f)] private float escapeSpeedMultiplier = 2.5f;
    [SerializeField, Min(1f)] private float repositionDistance = 6f;
    [SerializeField, Min(0.1f)] private float repositionCooldown = 2f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    private NavMeshAgent agent;

    private ATroop currentTarget;
    private ATroop dangerTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
    private float nextRepositionTime;

    private float normalSpeed;

    private bool repositioning;
    private bool returningToBase;
    private Vector3 repositionDestination;

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

        UpdateSpriteVisuals();

        // Actualizar periódicamente los objetivos.
        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateInterval;

            dangerTarget = CombatSystem.FindClosestEnemy(troop, dangerRadius);

            if (!IsInCombat)
            {
                currentTarget = CombatSystem.FindClosestEnemy(troop, detectionRadius);

                if (currentTarget != null)
                    returningToBase = false;
            }
        }

        if (repositioning && IsInCombat && currentTarget.GetDefinition().type == TroopType.SPRING_MINIBOSS)
        {
            FinishRepositioning();
        }

        // Si ya está reposicionándose, debe terminar el desplazamiento.
        if (repositioning)
        {
            HandleRepositioning();
            return;
        }

        // Si un enemigo se acerca demasiado, buscar otra posición.
        bool isBossTarget = IsInCombat && currentTarget.GetDefinition().type == TroopType.SPRING_MINIBOSS;

        if (!isBossTarget && CombatSystem.IsValidEnemy(troop, dangerTarget) && Time.time >= nextRepositionTime)
        {
            if (TryReposition())
                return;
        }

        // Atacar si tiene un objetivo válido.
        if (IsInCombat)
        {
            returningToBase = false;
            HandleCombat();
        }
        else
        {
            currentTarget = null;
            ReturnToEnemyBase();
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
        dangerTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextRepositionTime = 0f;

        repositioning = false;
        returningToBase = false;

        if (agent != null && troop != null && troop.GetDefinition() != null)
        {
            normalSpeed = troop.GetDefinition().speed;
            agent.speed = normalSpeed;
        }

        troop.SetMovementStopped(false);
    }

    public void ResetBehaviour()
    {
        currentTarget = null;
        dangerTarget = null;

        nextTargetUpdateTime = 0f;
        nextAttackTime = 0f;
        nextRepositionTime = 0f;

        repositioning = false;
        returningToBase = false;

        if (agent != null)
            agent.speed = normalSpeed;

        if (troop != null)
            troop.SetMovementStopped(false);
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

            if (!isShooting && Time.time >= nextAttackTime)
            {
                StartShooting();
            }
        }
        else
        {
            troop.SetMovementStopped(false);
            troop.SetTarget(currentTarget.transform);
        }
    }

    private void StartShooting()
    {
        if (isShooting || animator == null)
            return;

        if (!CombatSystem.IsValidEnemy(troop, currentTarget))
            return;

        isShooting = true;
        shootingTarget = currentTarget;

        nextAttackTime = Time.time + attackCooldown;

        animator.SetTrigger("shoot");
    }

    public void ShootProjectile()
    {
        if (!isShooting || projectilePrefab == null || projectileSpawnPoint == null)
            return;

        if (!CombatSystem.IsValidEnemy(troop, shootingTarget))
            return;

        GameObject projectileObject = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        RangedProjectile projectile = projectileObject.GetComponent<RangedProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(troop, shootingTarget, troop.GetDefinition().strength);
        }
        else
        {
            Destroy(projectileObject);
        }
    }

    public void FinishShooting()
    {
        isShooting = false;
        shootingTarget = null;

        if (animator != null)
            animator.ResetTrigger("shoot");
    }

    #endregion

    #region REPOSITIONING

    private bool TryReposition()
    {
        if (!CombatSystem.IsValidEnemy(troop, dangerTarget))
            return false;

        Vector3 awayDirection = (transform.position - dangerTarget.transform.position).normalized;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = -transform.right;

        // Probar varias direcciones para encontrar una posición válida.
        Vector3[] directions =
        {
            awayDirection,
            Quaternion.Euler(0f, 0f, 45f) * awayDirection,
            Quaternion.Euler(0f, 0f, -45f) * awayDirection,
            Quaternion.Euler(0f, 0f, 90f) * awayDirection,
            Quaternion.Euler(0f, 0f, -90f) * awayDirection,
            -awayDirection
        };

        foreach (Vector3 direction in directions)
        {
            Vector3 desiredPosition = transform.position + direction * repositionDistance;

            if (!TrySetValidDestination(desiredPosition))
                continue;

            repositioning = true;
            repositionDestination = agent.destination;

            nextRepositionTime = Time.time + repositionCooldown;

            agent.speed = normalSpeed * escapeSpeedMultiplier;
            agent.isStopped = false;

            troop.SetMovementStopped(false);

            return true;
        }

        return false;
    }

    private void HandleRepositioning()
    {
        if (!agent.pathPending && (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.remainingDistance <= agent.stoppingDistance + 0.2f))
        {
            FinishRepositioning();
        }
    }

    private void FinishRepositioning()
    {
        repositioning = false;

        agent.speed = normalSpeed;
        agent.ResetPath();
        agent.isStopped = false;

        // El siguiente Update recuperará el comportamiento de combate.
    }

    private bool TrySetValidDestination(Vector3 desiredPosition)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, navMeshSampleDistance, agent.areaMask))
            return false;

        NavMeshPath path = new NavMeshPath();

        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            return false;

        return agent.SetDestination(hit.position);
    }

    #endregion

    #region MOVEMENT

    private void ReturnToEnemyBase()
    {
        if (returningToBase)
            return;

        Faction enemyFaction = troop.GetFaction() == Faction.ALLY ? Faction.ENEMY : Faction.ALLY;

        Transform enemyBase = troop.FindBaseTransform(enemyFaction);

        if (enemyBase != null)
        {
            returningToBase = true;
            troop.SetMovementStopped(false);
            troop.SetTarget(enemyBase);
        }
    }

    #endregion

    #region VISUALS

    private void UpdateSpriteVisuals()
    {
        if (agent == null)
            return;

        SpriteRenderer sprite = troop.GetSpriteRenderer();

        if (sprite != null && Mathf.Abs(agent.velocity.x) > 0.05f)
            sprite.flipX = agent.velocity.x < 0f;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerRadius);
    }
}