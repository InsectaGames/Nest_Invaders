
using BehaviourAPI.Core;
using UnityEngine;
using UnityEngine.AI;

public class GathererTroop : MonoBehaviour, ITroop
{
    private enum GathererState
    {
        Inactive,
        WaitingForTargets,
        MovingToGatherPoint,
        Gathering,
        ReturningToBase
    }

    private ATroop troop;
    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    public TroopType Type => TroopType.GATHERER;

    private GathererState state = GathererState.Inactive;

    private Transform gatherPointTransform;
    private Transform currentTarget;

    [Header("Recolección")]
    [SerializeField] private float gatheringDuration = 5f;
    [SerializeField] private int minAmount = 2;
    [SerializeField] private int maxAmount = 4;

    [Header("Movimiento")]
    [SerializeField] private float arrivalTolerance = 0.3f;
    [SerializeField] private float movementRetryInterval = 1.5f;

    private float gatheringTimer;
    private float movementRetryTimer;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (troop == null || troop.GetDefinition() == null)
            return;

        if (state == GathererState.WaitingForTargets)
        {
            TryInitializeTargets();
            return;
        }

        if (state == GathererState.Gathering)
        {
            UpdateGathering();
            return;
        }

        if (state != GathererState.MovingToGatherPoint && state != GathererState.ReturningToBase)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        UpdateMovement();
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

        state = GathererState.WaitingForTargets;

        gatheringTimer = 0f;
        movementRetryTimer = 0f;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        TryInitializeTargets();
    }

    private void TryInitializeTargets()
    {
        if (gatherPointTransform == null)
        {
            GameObject gatherPoint = GameObject.FindGameObjectWithTag("GatherPoint");

            if (gatherPoint != null)
                gatherPointTransform = gatherPoint.transform;
        }

        Transform allyBase = troop.GetAllyBaseTransform();

        if (gatherPointTransform == null || allyBase == null)
            return;

        BeginMovement(gatherPointTransform, GathererState.MovingToGatherPoint);
    }

    public void ResetBehaviour()
    {
        state = GathererState.Inactive;

        gatheringTimer = 0f;
        movementRetryTimer = 0f;

        gatherPointTransform = null;
        currentTarget = null;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    #endregion

    #region MOVEMENT

    private void BeginMovement(Transform target, GathererState newState)
    {
        if (target == null)
            return;

        currentTarget = target;
        state = newState;

        movementRetryTimer = 0f;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            troop.SetTarget(currentTarget);
        }
    }

    private void UpdateMovement()
    {
        agent.isStopped = false;

        if (currentTarget == null)
        {
            state = GathererState.WaitingForTargets;
            return;
        }

        if (HasReachedTarget())
        {
            if (state == GathererState.MovingToGatherPoint)
                BeginGathering();
            else if (state == GathererState.ReturningToBase)
                DeliverResources();

            return;
        }

        movementRetryTimer -= Time.deltaTime;

        if (movementRetryTimer > 0f || agent.pathPending)
            return;

        movementRetryTimer = movementRetryInterval;

        bool invalidPath = !agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete;

        bool stalled = agent.hasPath && agent.velocity.sqrMagnitude < 0.01f && agent.remainingDistance > agent.stoppingDistance + arrivalTolerance;

        if (invalidPath || stalled)
            troop.SetTarget(currentTarget);
    }

    private bool HasReachedTarget()
    {
        if (agent.pathPending)
            return false;

        if (agent.hasPath)
        {
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                return false;

            return agent.remainingDistance <= agent.stoppingDistance + arrivalTolerance;
        }

        // Respaldo para destinos que coinciden prácticamente con la posición actual del agente.
        return Vector3.Distance(transform.position, currentTarget.position) <= agent.stoppingDistance + arrivalTolerance;
    }

    #endregion

    #region GATHERING

    private void BeginGathering()
    {
        state = GathererState.Gathering;
        gatheringTimer = gatheringDuration;

        agent.isStopped = true;
        agent.ResetPath();

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private void UpdateGathering()
    {
        gatheringTimer -= Time.deltaTime;

        if (gatheringTimer <= 0f)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            Transform allyBase = troop.GetAllyBaseTransform();

            if (allyBase == null)
            {
                state = GathererState.WaitingForTargets;
                return;
            }

            BeginMovement(allyBase, GathererState.ReturningToBase);
        }
    }

    private void DeliverResources()
    {
        troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.LEAVES, Utils.GetRandomInclusive(minAmount, maxAmount)));

        BeginMovement(gatherPointTransform, GathererState.MovingToGatherPoint);
    }

    #endregion
}