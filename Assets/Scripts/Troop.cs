using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Troop : MonoBehaviour
{
    //Tipo de tropa
    [SerializeField] private TroopDefinition definition;

    //Componentes de vida y navmesh
    private NavMeshAgent agent;
    private HealthComponent health;
    private static GameObject[] bases;

    public TroopDefinition GetDefinition() => definition;

    private void Awake()
    {

        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();
        agent.updateUpAxis = false;
        Debug.Log("Se ha girado correctamente");
    }

    private void Start()
    {
        bases = GameObject.FindGameObjectsWithTag("Base"); 

    }

    public void Initialize(TroopDefinition troopDefinition)
    {
        definition = troopDefinition;

        if (agent != null)
        {
            agent.speed = definition.speed;

           foreach(GameObject _base in bases) { 

            if(definition.faction != _base.GetComponent<BaseSystem>().GetFaction()){
                    SetTarget(_base.transform);
                    break;
                }

            }
            



            // agent.updateRotation = false;
            agent.updateUpAxis = false;
            Debug.Log("Se ha girado correctamente");
        }

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