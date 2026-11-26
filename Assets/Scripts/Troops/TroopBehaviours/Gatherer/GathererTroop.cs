using UnityEngine;

public enum GathererState
{
    Inactive,
    WaitingForTargets,
    MovingToGatherPoint,
    Gathering,
    ReturningToBase
}

public class GathererTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Animator _anim;
    private GathererPathFinder pathfinder;

    public TroopType Type => TroopType.GATHERER;

    private GathererState state = GathererState.Inactive;
    public GathererState GetState() => state;

    private Transform gatherPointTransform;
    private Transform currentTarget;

    private Vector2[] currentPath;
    private int pathIndex;

    [Header("Recolección")]
    [SerializeField] private float gatheringDuration = 5f;
    [SerializeField] private int minAmount = 2;
    [SerializeField] private int maxAmount = 4;

    [Header("Movimiento")]
    [SerializeField] private float arrivalTolerance = 0.2f;
    [SerializeField] private float movementRetryInterval = 1.5f;

    private float gatheringTimer;
    private float movementRetryTimer;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        _anim = GetComponent<Animator>();
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

        if (state == GathererState.MovingToGatherPoint || state == GathererState.ReturningToBase)
        {
            UpdateMovement();
        }
    }

    #region INITIALIZATION

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        pathfinder = FindFirstObjectByType<GathererPathFinder>();

        state = GathererState.WaitingForTargets;
        gatheringTimer = 0f;
        movementRetryTimer = 0f;

        col.enabled = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        TryInitializeTargets();
    }

    private void TryInitializeTargets()
    {
        if (pathfinder == null)
            pathfinder = FindFirstObjectByType<GathererPathFinder>();

        if (pathfinder == null)
            return;

        if (gatherPointTransform == null)
        {
            GameObject gatherPoint = GameObject.FindGameObjectWithTag("GatherPoint");

            if (gatherPoint != null)
            {
                Transform marker = gatherPoint.transform.Find("GatherTarget");
                gatherPointTransform = marker != null ? marker : gatherPoint.transform;
            }
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
        currentPath = null;
        pathIndex = 0;

        if (col != null)
            col.enabled = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    #endregion

    #region MOVEMENT

    private Transform GetBaseGatherTarget()
    {
        Transform allyBase = troop.GetAllyBaseTransform();

        if (allyBase == null)
            return null;

        Transform marker = allyBase.Find("GathererTarget");
        return marker != null ? marker : allyBase;
    }

    private void BeginMovement(Transform target, GathererState newState)
    {
        if (target == null || pathfinder == null)
            return;

        currentTarget = target;
        state = newState;
        movementRetryTimer = 0f;
        currentPath = null;
        pathIndex = 0;

        TryCalculatePath();
    }

    private bool TryCalculatePath()
    {
        if (currentTarget == null || pathfinder == null)
            return false;

        if (!pathfinder.FindPath(transform.position, currentTarget.position, out Vector2[] path))
            return false;

        currentPath = path;
        pathIndex = 0;
        return true;
    }

    private void UpdateMovement()
    {
        if (currentTarget == null)
        {
            state = GathererState.WaitingForTargets;
            return;
        }

        if (currentPath == null || pathIndex >= currentPath.Length)
        {
            if (currentPath != null && pathIndex >= currentPath.Length)
            {
                OnReachedTarget();
                return;
            }

            movementRetryTimer -= Time.deltaTime;

            if (movementRetryTimer <= 0f)
            {
                movementRetryTimer = movementRetryInterval;
                TryCalculatePath();
            }

            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 waypoint = currentPath[pathIndex];

        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, waypoint, troop.GetDefinition().speed * Time.deltaTime);

        Vector2 direction = nextPosition - currentPosition;

        transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);

        if (Mathf.Abs(direction.x) > 0.001f && spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0f;

        if (Vector2.Distance(nextPosition, waypoint) <= arrivalTolerance)
            pathIndex++;
    }

    private void OnReachedTarget()
    {
        currentPath = null;

        if (state == GathererState.MovingToGatherPoint)
            BeginGathering();
        else if (state == GathererState.ReturningToBase)
            DeliverResources();
    }

    #endregion

    #region GATHERING

    private void BeginGathering()
    {
        state = GathererState.Gathering;
        gatheringTimer = gatheringDuration;

        col.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        _anim?.SetBool("has_fungus", true);
    }

    private void UpdateGathering()
    {
        gatheringTimer -= Time.deltaTime;

        if (gatheringTimer > 0f)
            return;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        Transform allyBaseTarget = GetBaseGatherTarget();

        if (allyBaseTarget == null)
        {
            state = GathererState.WaitingForTargets;
            return;
        }

        col.enabled = true;
        BeginMovement(allyBaseTarget, GathererState.ReturningToBase);
    }

    private void DeliverResources()
    {
        troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.FUNGUS, Utils.GetRandomInclusive(minAmount, maxAmount)));

        _anim?.SetBool("has_fungus", false);

        BeginMovement(gatherPointTransform, GathererState.MovingToGatherPoint);
    }

    #endregion
}