using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Troop : MonoBehaviour
{
    // Tipo de tropa.
    [SerializeField] private TroopDefinition definition;

    // Componentes de vida y navmesh.
    private NavMeshAgent agent;
    private HealthComponent health;
    private static GameObject[] bases;

    public TroopDefinition GetDefinition() => definition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();
        bases = GameObject.FindGameObjectsWithTag("Base");

        agent.updateUpAxis = false;
        Debug.Log("Se ha girado correctamente");
    }

    public void Initialize(TroopDefinition troopDefinition)
    {
        definition = troopDefinition;

        if (agent != null)
        {
            agent.speed = definition.speed;

            if(definition.type != TroopType.GATHERER)
            {
                foreach(GameObject _base in bases)
                { 
                    if(definition.faction != _base.GetComponent<BaseSystem>().GetFaction())
                    {
                        SetTarget(_base.transform);
                        break;
                    }
                }
            }
            else
            {
                // Habría que establecer también para que regrese a la base, pille recursos...
                SetTarget(GameObject.FindGameObjectWithTag("GatherPoint").transform);
            }

            // agent.updateRotation = false;
            agent.updateUpAxis = false;
            Debug.Log("Se ha girado correctamente.");
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

    // Cuando entra en colisión con otro enemigo.
    private void OnCollisionEnter2D(Collision2D col)
    {
        
    }

    // Cuando entra en el trigger de una base.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Base"))
        {
            GameObject baseObj = col.gameObject;
            BaseSystem baseCol = baseObj.GetComponent<BaseSystem>();
            HealthComponent baseHealth = baseObj.GetComponent<HealthComponent>();

            if(this.definition.faction == Faction.ALLY && baseCol.GetFaction() == Faction.ENEMY ||
                this.definition.faction == Faction.ENEMY && baseCol.GetFaction() == Faction.ALLY)
            {
                baseHealth.TakeDamage();
                Destroy(this.gameObject);
            }
        }
    }
}