using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Troop : ASubject<GameEvent<ResourceSlot>>
{
    // Tipo de tropa.
    [SerializeField] private TroopDefinition definition;
    private Faction faction;

    // Componentes de vida y navmesh.
    private NavMeshAgent agent;
    private HealthComponent health;

    private static GameObject[] bases;

    private bool isGathering = false;

    private Transform allyBaseTransform;
    private Transform gatherPointTransform;

    /// <summary>
    /// Provisional para el ataque cuando hay colision hasta que usemos los scriptable objects como tal
    /// </summary>
    [Header("Combate melee")]
    [SerializeField] private int meleeCollisionDamage = 9999;

    public TroopDefinition GetDefinition() => definition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();

        bases = GameObject.FindGameObjectsWithTag("Base");
        Debug.Log(bases.Length);
        foreach (GameObject obj in bases)
        {
            BaseSystem bs = obj.GetComponent<BaseSystem>();
            if (bs.GetFaction() == Faction.ALLY)
            {
                this.AddObserver(bs);
                break;
            }
        }

        //this.AddObserver(GameObject.FindAnyObjectByType<UIFungusCounter>());

        //agent.updateUpAxis = false;
        Debug.Log("Se ha girado correctamente");
    }

    public void Initialize(TroopDefinition troopDefinition, Faction f)
    {
        definition = troopDefinition;
        faction = f;

        if (agent != null)
        {
            agent.speed = definition.speed;
            FindAllyBase(f);

            if (definition.type != TroopType.GATHERER)
            {
                foreach (GameObject _base in bases)
                {
                    if (faction != _base.GetComponent<BaseSystem>().GetFaction())
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
        /*
        if (TroopsManager.Instance != null)
        {
            if (f == Faction.ALLY)
            {
                GameEvent<ResourceSlot> placementEvent = new GameEvent<ResourceSlot>(LogicEvent.TROOP_PLACED, new ResourceSlot(Resource.FUNGUS, 1));
                UpdateObservers(placementEvent);
            }

            TroopsManager.Instance.AddTroop(gameObject);
        }
        */
    }

    private void FindAllyBase(Faction f = Faction.ALLY)
    {
        if (bases == null) return;

        foreach (GameObject _base in bases)
        {
            BaseSystem baseSystem = _base.GetComponent<BaseSystem>();
            if (baseSystem != null && baseSystem.GetFaction() == f)
            {
                faction = baseSystem.GetFaction();
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
    // Provisional de momento hasta que haya comportamientos de peleas en el juego
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (definition == null || (definition.type != TroopType.MELEE_ATTACKER))
            return;

        Troop otherTroop = col.gameObject.GetComponent<Troop>();
        if (otherTroop == null)
            return;

        TroopDefinition otherDefinition = otherTroop.GetDefinition();
        if (otherDefinition == null || otherTroop.faction == faction)
            return;

        HealthComponent otherHealth = col.gameObject.GetComponent<HealthComponent>();

        if (otherHealth == null || otherHealth.IsDead())
            return;

        otherHealth.TakeDamage(meleeCollisionDamage);

        Debug.Log($"{definition.Name} ({faction}) ha golpeado a " + $"{otherDefinition.Name} ({otherTroop.faction}). " + $"Daño: {meleeCollisionDamage}. " + $"Vida restante: {otherHealth.Health}.");

        if (otherHealth.IsDead())
        {
            otherHealth.Die();
        }
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

            if (definition.type == TroopType.GATHERER && isGathering && baseCol.GetFaction() == faction)
            {
                GameEvent<ResourceSlot> resourceEvent = new GameEvent<ResourceSlot>(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.FUNGUS, 1));
                UpdateObservers(resourceEvent);

                Debug.Log("Recursos entregados en la base. Volviendo a la mina...");
                isGathering = false;

                if (gatherPointTransform != null)
                {
                    SetTarget(gatherPointTransform);
                }
            }
            else if (faction == Faction.ALLY && baseCol.GetFaction() == Faction.ENEMY || faction == Faction.ENEMY && baseCol.GetFaction() == Faction.ALLY)
            {
                baseHealth.TakeDamage();
                Destroy(this.gameObject);
            }
        }
    }

    private IEnumerator GatherRoutine()
    {
        isGathering = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        SpriteRenderer spr = this.GetComponent<SpriteRenderer>();
        Collider2D collider = this.GetComponent<Collider2D>();

        // Entra en la cueva.
        spr.enabled = false;
        collider.enabled = false;

        yield return new WaitForSeconds(5f);

        if (agent != null)
        {
            agent.enabled = true;
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