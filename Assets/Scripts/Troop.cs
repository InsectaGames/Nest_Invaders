using UnityEngine;
using UnityEngine.AI;

public class Troop : MonoBehaviour
{
    //Tipo de tropa
    [SerializeField] private TroopDefinition definition;

    //Componentes de vida y navmesh
    private NavMeshAgent agent;
    private HealthComponent health;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();
    }

    public void Initialize(TroopDefinition troopDefinition)
    {
        definition = troopDefinition;

        if (agent != null)
            agent.speed = definition.speed;

        if (health != null)
            health.Initialize(definition.maxHealth);
    }

    public void SetTarget(Transform target)
    {
        if (target == null)
            return;

        if (agent == null)
            return;

        agent.SetDestination(target.position);
    }
}