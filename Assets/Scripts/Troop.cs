using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class Troop : ASubject<GameEvent<ResourceSlot>>
{
    // Tipo de tropa.
    [SerializeField] private TroopDefinition definition;

    // Componentes de vida y navmesh.
    private NavMeshAgent agent;
    private HealthComponent health;

    private static GameObject[] bases;

    private bool isGathering = false;

    private Transform allyBaseTransform;
    private Transform gatherPointTransform;

    public TroopDefinition GetDefinition() => definition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();
        bases = GameObject.FindGameObjectsWithTag("Base");
        Debug.Log(bases.Length);
        foreach(GameObject obj in bases)
        {
            BaseSystem bs = obj.GetComponent<BaseSystem>();
            if(bs.GetFaction() == Faction.ALLY)
            {
                this.AddObserver(bs);
                break;
            }
        }

        GameObject.Find("OperativeCentral").GetComponent<TroopsManager>().AddTroop(this.gameObject);

        //agent.updateUpAxis = false;
        Debug.Log("Se ha girado correctamente");
    }

    public void Initialize(TroopDefinition troopDefinition)
    {
        definition = troopDefinition;

        if (agent != null)
        {
            agent.speed = definition.speed;
            FindAllyBase();

            if (definition.type != TroopType.GATHERER)
            {
                foreach (GameObject _base in bases)
                {
                    if (definition.faction != _base.GetComponent<BaseSystem>().GetFaction())
                    {
                        SetTarget(_base.transform);
                        break;
                    }
                }
            }
            else
            {
                // Habría que establecer también para que regrese a la base, pille recursos...
                GameObject gatherPoint = GameObject.FindGameObjectWithTag("GatherPoint");

                if (gatherPoint != null)
                {
                    gatherPointTransform = gatherPoint.transform;
                    SetTarget(gatherPointTransform);
                }
            }

            // agent.updateRotation = false;
            agent.updateUpAxis = false;
            Debug.Log("Se ha girado correctamente.");
        }

        if (health != null)
            health.Initialize(definition.maxHealth);
    }

    private void FindAllyBase()
    {
        if (bases == null) return;

        foreach (GameObject _base in bases)
        {
            BaseSystem baseSystem = _base.GetComponent<BaseSystem>();
            if (baseSystem != null && baseSystem.GetFaction() == this.definition.faction)
            {
                allyBaseTransform = _base.transform;
                break;
            }
        }
    }

    public void SetTarget(Transform target)
    {
        if (target == null || agent == null)
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

        // Detecta cuando el Gatherer llega al GatherPoint por primera vez
        if (definition.type == TroopType.GATHERER && !isGathering && col.CompareTag("GatherPoint"))
        {
            StartCoroutine(GatherRoutine());
            return;
        }

        if (col.gameObject.CompareTag("Base"))
        {
            GameObject baseObj = col.gameObject;
            BaseSystem baseCol = baseObj.GetComponent<BaseSystem>();
            HealthComponent baseHealth = baseObj.GetComponent<HealthComponent>();

            if (definition.type == TroopType.GATHERER && isGathering && baseCol.GetFaction() == this.definition.faction)
            {
                GameEvent<ResourceSlot> resourceEvent = new GameEvent<ResourceSlot>(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.LEAF, 1));
                UpdateObservers(resourceEvent);

                Debug.Log("Recursos entregados en la base. Volviendo a la mina...");
                isGathering = false;

                if (gatherPointTransform != null)
                {
                    SetTarget(gatherPointTransform);
                }
            }
            else if (this.definition.faction == Faction.ALLY && baseCol.GetFaction() == Faction.ENEMY ||
                            this.definition.faction == Faction.ENEMY && baseCol.GetFaction() == Faction.ALLY)
            {
                baseHealth.TakeDamage();
                Destroy(this.gameObject);
            }
        }
    }

   // [System.Obsolete]
   IEnumerator GatherRoutine()
    {
        isGathering = true;

        if (agent != null)
            agent.isStopped = true;

        SpriteRenderer spr = this.GetComponent<SpriteRenderer>();
        Collider2D collider = this.GetComponent<Collider2D>();

        //Hacemos como que entra en la cueva ,  en ese momento , no es interactuable
        spr.enabled = false;
        collider.enabled = false;
        
        yield return new WaitForSeconds(5f);

        if (agent != null)
        {
         

            agent.isStopped = false;
            spr.enabled = true;
            collider.enabled = true;

            if (allyBaseTransform != null)
            {
                SetTarget(allyBaseTransform);
            }
        }
    }
}