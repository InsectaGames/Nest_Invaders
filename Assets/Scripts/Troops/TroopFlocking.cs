
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ATroop))]
[RequireComponent(typeof(NavMeshAgent))]
public class TroopFlocking : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float neighbourRadius = 3.5f;
    [SerializeField] private float separationRadius = 2f;
    [SerializeField] private LayerMask troopLayers = ~0;

    [Header("Pesos")]
    [SerializeField] private float separationStrength = 3.5f;
    [SerializeField] private float alignmentStrength = 0f;

    [Header("Movimiento")]
    [SerializeField] private float correctionDistance = 1.5f;
    [SerializeField] private float updateInterval = 0.1f;

    private ATroop troop;
    private NavMeshAgent agent;

    private Vector3 originalDestination;
    private Vector3 lastAppliedDestination;

    private float updateTimer;
    private bool initialized;
    private bool hasDestination;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        agent = GetComponent<NavMeshAgent>();

        enabled = false;
    }

    public void Initialize()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        initialized = true;
        hasDestination = false;
        updateTimer = 0f;

        enabled = true;
    }

    public void ResetFlocking()
    {
        initialized = false;
        hasDestination = false;
        enabled = false;
    }

    private void Update()
    {
        if (!initialized ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh ||
            agent.isStopped)
        {
            return;
        }

        if (!agent.hasPath)
        {
            hasDestination = false;
            return;
        }

        // Recuperar el destino principal cuando cambia.
        if (!hasDestination ||
            Vector3.Distance(agent.destination, lastAppliedDestination) > 0.5f)
        {
            originalDestination = agent.destination;
            hasDestination = true;
        }

        updateTimer -= Time.deltaTime;

        if (updateTimer > 0f)
            return;

        updateTimer = updateInterval;

        Vector2 correction = CalculateCorrection();

        if (correction.sqrMagnitude < 0.001f)
        {
            agent.SetDestination(originalDestination);
            lastAppliedDestination = originalDestination;
            return;
        }

        Vector2 correctedDestination =
            (Vector2)originalDestination +
            correction.normalized * correctionDistance;

        Vector3 desiredPosition = new Vector3(
            correctedDestination.x,
            correctedDestination.y,
            originalDestination.z
        );

        // Asegurar que el nuevo destino pertenece al NavMesh.
        if (NavMesh.SamplePosition(
            desiredPosition,
            out NavMeshHit hit,
            correctionDistance + 1f,
            agent.areaMask))
        {
            agent.SetDestination(hit.position);
            lastAppliedDestination = hit.position;
        }
    }

    private Vector2 CalculateCorrection()
    {
        Collider2D[] neighbours = Physics2D.OverlapCircleAll(
            transform.position,
            neighbourRadius,
            troopLayers
        );

        Vector2 separation = Vector2.zero;
        Vector2 alignment = Vector2.zero;

        int neighbourCount = 0;
        int separationCount = 0;

        Vector2 myPosition = transform.position;

        foreach (Collider2D col in neighbours)
        {
            if (col == null)
                continue;

            ATroop otherTroop = col.GetComponentInParent<ATroop>();

            if (otherTroop == null || otherTroop == troop)
                continue;

            if (otherTroop.GetFaction() != troop.GetFaction())
                continue;

            NavMeshAgent otherAgent =
                otherTroop.GetComponent<NavMeshAgent>();

            if (otherAgent == null ||
                !otherAgent.enabled ||
                !otherAgent.isOnNavMesh)
                continue;

            Vector2 difference =
                myPosition - (Vector2)otherTroop.transform.position;

            float distance = difference.magnitude;

            if (distance <= 0.001f || distance > neighbourRadius)
                continue;

            neighbourCount++;

            if (distance < separationRadius)
            {
                separation += difference.normalized / distance;
                separationCount++;
            }

            alignment += new Vector2(
                otherAgent.velocity.x,
                otherAgent.velocity.y
            );
        }

        if (neighbourCount == 0)
            return Vector2.zero;

        Vector2 steering = Vector2.zero;

        if (separationCount > 0)
        {
            separation /= separationCount;
            steering += separation.normalized * separationStrength;
        }

        alignment /= neighbourCount;

        if (alignment.sqrMagnitude > 0.001f)
            steering += alignment.normalized * alignmentStrength;

        return Vector2.ClampMagnitude(steering, 1f);
    }

    private void OnDisable()
    {
        hasDestination = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, neighbourRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}