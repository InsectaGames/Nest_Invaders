
using System.Collections.Generic;
using UnityEngine;

public class SpringMiniBossTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    private HealthComponent health;

    public TroopType Type => TroopType.SPRING_MINIBOSS;

    [Header("Zona de patrulla")]
    [SerializeField] private Vector2 mapCenter = new Vector2(-0.0543f, 0.0729f);
    [SerializeField, Min(0.1f)] private float patrolBaseClearance = 1f;
    [SerializeField, Min(0.1f)] private float patrolMajorAxis = 3f;
    [SerializeField, Min(0.1f)] private float patrolMinorAxis = 1.5f;
    [SerializeField, Range(0.1f, 0.95f)] private float returnStart = 0.65f;

    [Header("Movimiento")]
    [SerializeField, Min(0.1f)] private float maxRandomTurnRate = 35f;
    [SerializeField, Min(0.1f)] private float returnTurnSpeed = 55f;
    [SerializeField, Min(0.1f)] private float turnChangeInterval = 1.5f;

    [Header("Ataque en área")]
    [SerializeField, Min(0.1f)] private float attackCooldown = 3f;

    [Header("Estado de combate")]
    [SerializeField, Min(0.1f)] private float combatDetectionRadius = 3f;
    [SerializeField, Min(0f)] private float combatExitCooldown = 2f;

    private bool isInCombat;
    private float combatResumeTime;

    [Header("Refuerzos")]
    [SerializeField, Range(0.01f, 1f)] private float reinforcementHealthThreshold = 0.5f;
    [SerializeField, Min(1)] private int reinforcementCount = 3;

    [Header("Destrucción de segmentos")]
    [SerializeField] private CentipedeBodyFollow bodyFollow;

    private int lastBodySegmentCount = -1;

    private Transform enemyBase;
    private BaseSystem enemyBaseSystem;

    private Vector2 inwardDirection;
    private Vector2 lateralDirection;
    private Vector2 patrolCenter;
    private Vector2 movementDirection;

    private float currentTurnRate;
    private float nextTurnChangeTime;
    private float nextAttackTime;

    private bool reinforcementsSpawned;
    private bool isAttacking;
    private bool isReturning;
    private bool patrolInitialized;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        health = GetComponent<HealthComponent>();
        bodyFollow = GetComponent<CentipedeBodyFollow>();
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

        UpdateBodySegments();
        CheckReinforcementThreshold();

        if (HasCombatTargets())
        {
            isInCombat = true;
            combatResumeTime = Time.time + combatExitCooldown;
        }
        else if (isInCombat && Time.time >= combatResumeTime)
        {
            isInCombat = false;
        }

        if (isAttacking)
            return;

        TryAreaAttack();

        if (!isAttacking)
            HandlePatrol();
    }

    private void UpdateBodySegments()
    {
        if (bodyFollow == null || health == null || health.MaxHealth <= 0)
            return;

        float healthPercentage = (float)health.Health / health.MaxHealth;
        int totalSegments = bodyFollow.BodySegmentCount;
        int desiredSegments = Mathf.Clamp(Mathf.FloorToInt(healthPercentage * totalSegments), 0, totalSegments);

        if (desiredSegments == lastBodySegmentCount)
            return;

        lastBodySegmentCount = desiredSegments;
        bodyFollow.SetActiveBodySegmentCount(desiredSegments);
    }

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (health == null)
            health = GetComponent<HealthComponent>();

        if (bodyFollow == null)
            bodyFollow = GetComponent<CentipedeBodyFollow>();

        FindBaseReferences();

        lastBodySegmentCount = -1;
        nextAttackTime = 0f;
        isInCombat = false;
        combatResumeTime = 0f;

        reinforcementsSpawned = false;
        isAttacking = false;
        isReturning = false;
        patrolInitialized = false;

        InitializePatrol();
    }

    public void ResetBehaviour()
    {
        enemyBase = null;
        enemyBaseSystem = null;

        nextAttackTime = 0f;
        reinforcementsSpawned = false;
        isInCombat = false;
        combatResumeTime = 0f;

        isAttacking = false;
        isReturning = false;
        patrolInitialized = false;
    }

    private void FindBaseReferences()
    {
        BaseSystem[] sceneBases = FindObjectsByType<BaseSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        enemyBase = null;
        enemyBaseSystem = null;

        foreach (BaseSystem baseSystem in sceneBases)
        {
            if (baseSystem == null)
                continue;

            if (baseSystem.GetFaction() != Faction.ENEMY)
                continue;

            enemyBase = baseSystem.transform;
            enemyBaseSystem = baseSystem;
        }
    }

    #region MERODEO

    private void InitializePatrol()
    {
        if (enemyBase == null)
            return;

        Vector2 basePosition = enemyBase.position;
        inwardDirection = (mapCenter - basePosition).normalized;

        if (inwardDirection.sqrMagnitude < 0.001f)
            inwardDirection = Vector2.left;

        // El eje mayor es paralelo a la base; el menor apunta hacia el centro.
        lateralDirection = new Vector2(-inwardDirection.y, inwardDirection.x);
        patrolCenter = basePosition + inwardDirection * (patrolMinorAxis + patrolBaseClearance);

        movementDirection = bodyFollow != null ? bodyFollow.GetHeadForwardDirection() : (Vector2)transform.right;

        if (movementDirection.sqrMagnitude < 0.001f)
            movementDirection = inwardDirection;

        movementDirection.Normalize();

        currentTurnRate = Random.Range(-maxRandomTurnRate, maxRandomTurnRate);
        nextTurnChangeTime = Time.time + Random.Range(turnChangeInterval * 0.5f, turnChangeInterval * 1.5f);

        patrolInitialized = true;
    }

    private void HandlePatrol()
    {
        if (enemyBase == null)
        {
            FindBaseReferences();
            InitializePatrol();
        }

        if (!patrolInitialized)
            return;

        Vector2 position = transform.position;
        Vector2 offset = position - patrolCenter;

        float lateralPosition = Vector2.Dot(offset, lateralDirection);
        float inwardPosition = Vector2.Dot(offset, inwardDirection);

        float normalizedDistance = (lateralPosition * lateralPosition) / (patrolMajorAxis * patrolMajorAxis);
        normalizedDistance += (inwardPosition * inwardPosition) / (patrolMinorAxis * patrolMinorAxis);

        if (!isReturning && normalizedDistance >= returnStart)
            isReturning = true;
        else if (isReturning && normalizedDistance <= returnStart * 0.2f)
            isReturning = false;

        bool returning = isReturning;

        if (returning)
        {
            Vector2 directionToCenter = (patrolCenter - position).normalized;
            float desiredAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;

            float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, returnTurnSpeed * Time.deltaTime);
            float radians = newAngle * Mathf.Deg2Rad;

            movementDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
        else
        {
            if (Time.time >= nextTurnChangeTime)
            {
                currentTurnRate = Random.Range(-maxRandomTurnRate, maxRandomTurnRate);
                nextTurnChangeTime = Time.time + Random.Range(turnChangeInterval * 0.5f, turnChangeInterval * 1.5f);
            }

            float angle = currentTurnRate * Time.deltaTime;
            movementDirection = Quaternion.Euler(0f, 0f, angle) * movementDirection;
        }

        float speed = troop.GetDefinition().speed;
        transform.position += (Vector3)(movementDirection * speed * Time.deltaTime);
    }

    #endregion

    #region ATAQUE EN AREA

    private bool HasCombatTargets()
    {
        float radius = Mathf.Max(combatDetectionRadius, troop.GetDefinition().attackReach);
        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop target = col.GetComponentInParent<ATroop>();

            if (target == null || target == troop || target.GetFaction() != Faction.ALLY)
                continue;

            HealthComponent targetHealth = target.GetHealth();

            if (targetHealth != null && !targetHealth.IsDead())
                return true;
        }

        return false;
    }

    private void TryAreaAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, troop.GetDefinition().attackReach);

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop target = col.GetComponentInParent<ATroop>();

            if (target == null || target == troop || target.GetFaction() != Faction.ALLY)
                continue;

            HealthComponent targetHealth = target.GetHealth();

            if (targetHealth == null || targetHealth.IsDead())
                continue;

            isAttacking = true;
            nextAttackTime = Time.time + attackCooldown;

            if (bodyFollow != null)
                bodyFollow.AttackAnimation(true);

            return;
        }
    }

    public void FinishAreaAttackAnimation()
    {
        if (!isAttacking)
            return;

        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, troop.GetDefinition().attackReach);
        HashSet<HealthComponent> targets = new HashSet<HealthComponent>();

        foreach (Collider2D col in detected)
        {
            if (col == null)
                continue;

            ATroop target = col.GetComponentInParent<ATroop>();

            if (target == null || target == troop || target.GetFaction() != Faction.ALLY)
                continue;

            HealthComponent targetHealth = target.GetHealth();

            if (targetHealth == null || targetHealth.IsDead())
                continue;

            targets.Add(targetHealth);
        }

        foreach (HealthComponent targetHealth in targets)
        {
            targetHealth.TakeDamage(troop.GetDefinition().strength);

            if (targetHealth.IsDead())
                targetHealth.Die();
        }

        if (bodyFollow != null)
            bodyFollow.AttackAnimation(false);

        isAttacking = false;
    }

    #endregion

    #region REFUERZOS

    private void CheckReinforcementThreshold()
    {
        if (reinforcementsSpawned || health == null || health.MaxHealth <= 0)
            return;

        int thresholdHealth = Mathf.CeilToInt(health.MaxHealth * reinforcementHealthThreshold);

        if (health.Health > thresholdHealth)
            return;

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
        if (troop != null && troop.GetDefinition() != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, troop.GetDefinition().attackReach);
        }

        Vector2 basePosition = enemyBase != null ? (Vector2)enemyBase.position : Vector2.zero;
        Vector2 inward = (mapCenter - basePosition).normalized;

        if (inward.sqrMagnitude < 0.001f)
            inward = Vector2.left;

        Vector2 lateral = new Vector2(-inward.y, inward.x);
        Vector2 center = basePosition + inward * (patrolMinorAxis + patrolBaseClearance);

        Gizmos.color = Color.yellow;

        Vector3 previous = Vector3.zero;

        for (int i = 0; i <= 64; i++)
        {
            float angle = i / 64f * Mathf.PI * 2f;
            Vector2 point = center + lateral * (Mathf.Cos(angle) * patrolMajorAxis) + inward * (Mathf.Sin(angle) * patrolMinorAxis);

            if (i > 0)
                Gizmos.DrawLine(previous, point);

            previous = point;
        }
    }
}