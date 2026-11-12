
using UnityEngine;
using UnityEngine.AI;

public class RangedTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;

    public TroopType Type => TroopType.RANGED_ATTACKER;

    [Header("Detección")]
    [SerializeField, Min(1f)] private float detectionRadius = 10f;
    [SerializeField, Min(0.1f)] private float dangerRadius = 3f;
    [SerializeField, Min(0.05f)] private float targetUpdateInterval = 0.2f;

    [Header("Combate")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, currentTarget);

    [Header("Huida")]
    [SerializeField, Min(1f)] private float escapeSpeedMultiplier = 1.5f;
    [SerializeField, Min(0.5f)] private float safeDistance = 7f;
    [SerializeField, Min(0.1f)] private float escapeUpdateInterval = 0.3f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    private NavMeshAgent agent;

    private ATroop currentTarget;
    private ATroop dangerTarget;

    private float nextTargetUpdateTime;
    private float nextAttackTime;
    private float nextEscapeUpdateTime;

    private float normalSpeed;

    private bool escaping;
    private bool returningToBase;

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

        UpdateSpriteVisuals();

        // Actualizar periódicamente los objetivos.
        if (Time.time >= nextTargetUpdateTime)
        {
            nextTargetUpdateTime = Time.time + targetUpdateInterval;

            // Cuando huye, comprueba si todavía hay enemigos dentro de la distancia de seguridad.
            float escapeDetectionRadius = escaping ? safeDistance : dangerRadius;

            dangerTarget = CombatSystem.FindClosestEnemy(troop, escapeDetectionRadius);

            if (!IsInCombat)
            {
                currentTarget = CombatSystem.FindClosestEnemy(troop, detectionRadius);

                if (currentTarget != null)
                    returningToBase = false;
            }
        }

        // La huida siempre tiene prioridad.
        if (dangerTarget != null)
        {
            escaping = true;
            HandleEscape();
            return;
        }

        // Si ya no hay enemigos dentro de safeDistance, se recupera el comportamiento normal.
        if (escaping)
            StopEscaping();

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
        nextEscapeUpdateTime = 0f;

        escaping = false;
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
        nextEscapeUpdateTime = 0f;

        escaping = false;
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
            return;
        }

        // Si está dentro del alcance efectivo de ataque, dispara.
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
                    returningToBase = false;

                    troop.SetMovementStopped(false);
                }
            }
        }
        else
        {
            // El enemigo está detectado, pero fuera del alcance.
            troop.SetMovementStopped(false);
            troop.SetTarget(currentTarget.transform);
        }
    }

    #endregion

    #region ESCAPE

    private bool TrySetValidDestination(Vector3 desiredPosition)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            return false;

        NavMeshPath path = new NavMeshPath();

        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            return false;

        agent.SetDestination(hit.position);
        return true;
    }

    private void HandleEscape()
    {
        if (!CombatSystem.IsValidEnemy(troop, dangerTarget))
            return;

        agent.speed = normalSpeed * escapeSpeedMultiplier;
        troop.SetMovementStopped(false);

        if (Time.time < nextEscapeUpdateTime)
            return;

        nextEscapeUpdateTime = Time.time + escapeUpdateInterval;

        Vector3 escapeDirection = (transform.position - dangerTarget.transform.position).normalized;

        if (escapeDirection.sqrMagnitude < 0.01f)
            escapeDirection = -transform.right;

        Vector3 escapePosition = dangerTarget.transform.position + escapeDirection * safeDistance;

        if (!TrySetValidDestination(escapePosition))
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }

    private void StopEscaping()
    {
        escaping = false;
        dangerTarget = null;

        if (agent != null)
            agent.speed = normalSpeed;

        // El siguiente comportamiento decidirá si debe atacar o continuar avanzando hacia la base enemiga.
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}