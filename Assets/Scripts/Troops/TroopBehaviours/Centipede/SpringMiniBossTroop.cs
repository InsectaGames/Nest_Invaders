
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpringMiniBossTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    private NavMeshAgent agent;
    private HealthComponent health;

    public TroopType Type => TroopType.SPRING_MINIBOSS;

    [Header("Merodeo")]
    [SerializeField, Min(0.1f)] private float frontOffset = 3f;
    [SerializeField, Min(0.1f)] private float patrolRadius = 2f;
    [SerializeField, Min(0.1f)] private float patrolInterval = 3f;
    [SerializeField, Min(0.1f)] private float navMeshSampleDistance = 3f;

    [Header("Ataque en área")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 3f;

    [Header("Refuerzos")]
    [SerializeField, Range(0.01f, 1f)] private float reinforcementHealthThreshold = 0.5f;
    [SerializeField, Min(1)] private int reinforcementCount = 3;

    [Header("Destrucción de segmentos")]
    [SerializeField] private CentipedeBodyFollow bodyFollow;

    private int lastBodySegmentCount = -1;

    private Transform enemyBase;
    private Transform allyBase;
    private BaseSystem enemyBaseSystem;

    private float nextPatrolTime;
    private float nextAttackTime;

    private bool reinforcementsSpawned;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();
        bodyFollow = GetComponent<CentipedeBodyFollow>();
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        UpdateBodySegments();
        CheckReinforcementThreshold();
        TryAreaAttack();
        HandlePatrol();
    }

    private void UpdateBodySegments()
    {
        if (bodyFollow == null || health == null || health.MaxHealth <= 0)
            return;

        float healthPercentage = (float)health.Health / health.MaxHealth;

        int totalSegments = bodyFollow.BodySegmentCount;

        int desiredSegments = Mathf.FloorToInt(healthPercentage * totalSegments);

        desiredSegments = Mathf.Clamp(desiredSegments, 0, totalSegments);

        if (desiredSegments == lastBodySegmentCount)
            return;

        lastBodySegmentCount = desiredSegments;

        bodyFollow.SetActiveBodySegmentCount(desiredSegments);
    }

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (health == null)
            health = GetComponent<HealthComponent>();

        FindBaseReferences();

        lastBodySegmentCount = -1;
        nextPatrolTime = 0f;
        nextAttackTime = 0f;
        reinforcementsSpawned = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (enemyBase != null)
            SetRandomPatrolDestination();
    }

    public void ResetBehaviour()
    {
        enemyBase = null;
        allyBase = null;
        enemyBaseSystem = null;

        nextPatrolTime = 0f;
        nextAttackTime = 0f;
        reinforcementsSpawned = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    private void FindBaseReferences()
    {
        BaseSystem[] sceneBases = FindObjectsByType<BaseSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        enemyBase = null;
        allyBase = null;
        enemyBaseSystem = null;

        foreach (BaseSystem baseSystem in sceneBases)
        {
            if (baseSystem == null)
                continue;

            switch (baseSystem.GetFaction())
            {
                case Faction.ENEMY:
                    enemyBase = baseSystem.transform;
                    enemyBaseSystem = baseSystem;
                    break;

                case Faction.ALLY:
                    allyBase = baseSystem.transform;
                    break;
            }
        }
    }

    #region MERODEO

    private void HandlePatrol()
    {
        // Reintentar localizar las bases si todavía no existen las referencias.
        if (enemyBase == null || allyBase == null)
            FindBaseReferences();

        if (enemyBase == null || agent.pathPending)
            return;

        // Si todavía está desplazándose hacia su destino, mantenerlo.
        if (agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            return;
        }

        if (Time.time < nextPatrolTime)
            return;

        SetRandomPatrolDestination();
    }

    private void SetRandomPatrolDestination()
    {
        if (enemyBase == null || agent == null || !agent.isOnNavMesh)
            return;

        Vector3 forward;

        if (allyBase != null)
            forward = (allyBase.position - enemyBase.position).normalized;
        else
            forward = enemyBase.right;

        Vector3 lateral = new Vector3(-forward.y, forward.x, 0f);

        // Centro de patrulla situado delante de la base enemiga.
        Vector3 patrolCenter = enemyBase.position + forward * frontOffset;

        for (int i = 0; i < 10; i++)
        {
            float forwardOffset = Random.Range(-patrolRadius * 0.25f, patrolRadius * 0.25f);
            float lateralOffset = Random.Range(-patrolRadius, patrolRadius);

            Vector3 candidate = patrolCenter + forward * forwardOffset + lateral * lateralOffset;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, agent.areaMask))
            {
                continue;
            }

            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            agent.isStopped = false;
            agent.SetDestination(hit.position);

            nextPatrolTime = Time.time + patrolInterval;
            return;
        }

        // Si no encuentra una posición válida, volver a intentarlo más adelante.
        nextPatrolTime = Time.time + 1f;
    }

    #endregion

    #region ATAQUE EN AREA

    private void TryAreaAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, troop.GetDefinition().attackReach);

        HashSet<HealthComponent> targets = new HashSet<HealthComponent>();

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop target = col.GetComponentInParent<ATroop>();

            if (target == null || target == troop)
                continue;

            if (target.GetFaction() != Faction.ALLY)
                continue;

            HealthComponent targetHealth = target.GetHealth();

            if (targetHealth == null || targetHealth.IsDead())
                continue;

            targets.Add(targetHealth);
        }

        // No consumir el cooldown si no hay aliados que atacar.
        if (targets.Count == 0)
            return;

        foreach (HealthComponent targetHealth in targets)
        {
            targetHealth.TakeDamage(troop.GetDefinition().strength);

            if (targetHealth.IsDead())
                targetHealth.Die();
        }

        nextAttackTime = Time.time + attackCooldown;
    }

    #endregion

    #region REFUERZOS

    private void CheckReinforcementThreshold()
    {
        if (reinforcementsSpawned || health == null || health.MaxHealth <= 0)
            return;

        int thresholdHealth = Mathf.CeilToInt(
            health.MaxHealth * reinforcementHealthThreshold
        );

        if (health.Health > thresholdHealth)
            return;

        // Se marca antes de invocar para impedir invocaciones repetidas.
        reinforcementsSpawned = true;

        if (enemyBaseSystem == null)
            FindEnemyBaseSystem();

        if (enemyBaseSystem != null)
            enemyBaseSystem.SpawnBossReinforcements(reinforcementCount);
    }

    private void FindEnemyBaseSystem()
    {
        BaseSystem[] bases = FindObjectsByType<BaseSystem>(FindObjectsSortMode.None);

        foreach (BaseSystem baseSystem in bases)
        {
            if (baseSystem != null && baseSystem.GetFaction() == Faction.ENEMY)
            {
                enemyBaseSystem = baseSystem;
                return;
            }
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, troop.GetDefinition().attackReach);

        if (enemyBase != null)
        {
            Vector3 forward = allyBase != null ? (allyBase.position - enemyBase.position).normalized : enemyBase.right;

            Vector3 center = enemyBase.position + forward * frontOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, patrolRadius);
        }
    }
}