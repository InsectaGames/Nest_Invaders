using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Troop : ASubject<GameEvent<ResourceSlot>>, IPoolable
{
    // Tipo de tropa.
    [SerializeField] private TroopDefinition definition;
    private Faction faction;

    private TroopPoolManager poolManager;
    private bool returningToPool = true;

    // Componentes de vida y NavMesh.
    private NavMeshAgent agent;
    private HealthComponent health;

    private static GameObject[] bases;

    private bool isGathering = false;

    private Transform allyBaseTransform;
    private Transform gatherPointTransform;

    /// <summary>
    /// Provisional para el ataque cuando hay colision hasta que usemos los scriptable objects como tal.
    /// </summary>
    [Header("Combate melee")]
    [SerializeField] private int meleeCollisionDamage = 9999;

    public TroopDefinition GetDefinition() => definition;
    public Faction GetFaction() => faction;

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
        
        Debug.Log("Se ha girado correctamente");
    }

    public void Initialize(TroopDefinition troopDefinition, Faction f, TroopPoolManager owner)
    {
        if(troopDefinition == null) return;

        definition = troopDefinition;
        faction = f;
        poolManager = owner;

        returningToPool = false;
        isGathering = false;

        // Restaurar la apariencia de la tropa.
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            if (definition._sprites != null &&
                definition._sprites.Length > 0)
            {
                spriteRenderer.sprite = definition._sprites[0];
            }

            spriteRenderer.color = (faction == Faction.ALLY) ? Color.white : Color.red;
            spriteRenderer.enabled = true;
        }

        Collider2D troopCollider = GetComponent<Collider2D>();

        if (troopCollider != null)
            troopCollider.enabled = true;

        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.speed = definition.speed;
            agent.isStopped = false;
            agent.updateUpAxis = false;
            Debug.Log("Se ha girado correctamente.");

            FindAllyBase(f);

            if (agent.isOnNavMesh)
                agent.ResetPath();

            if (definition.type != TroopType.GATHERER)
            {
                foreach (GameObject baseObject in bases)
                {
                    if (baseObject == null)
                        continue;

                    BaseSystem baseSystem =
                        baseObject.GetComponent<BaseSystem>();

                    if (baseSystem != null &&
                        baseSystem.GetFaction() != faction)
                    {
                        SetTarget(baseObject.transform);
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
            if(otherTroop.GetFaction() != Faction.ALLY)
                GambleForSticks(otherDefinition);

            otherHealth.Die();
        }
    }

    // Método para conseguir palos. De momento, simplemente será una tirada aleatoria en todos los enemigos.
    // En el futuro, solo se debe aplicar a los bichos que suelten palos.
    private void GambleForSticks(TroopDefinition def)
    {
        if(Utils.GetRandomInclusive(0, 100) > def.chanceToDropSticks)
        {
            GameEvent<ResourceSlot> resourceEvent = new GameEvent<ResourceSlot>(LogicEvent.RESOURCE_GATHER_2, new ResourceSlot(Resource.STICK, 1));
            UpdateObservers(resourceEvent);
        }
    }

    // Cuando entra en el trigger de una base.
    private void OnTriggerEnter2D(Collider2D col)
    {
        // Detecta cuando el Gatherer llega al GatherPoint por primera vez.
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
                ReturnToPool();
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

    #region OBJECT POOL
    public void OnSpawn()
    {
        // El objeto permanece inactivo hasta que TroopPoolManager lo coloque y lo inicialice.
        returningToPool = true;
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public void ResetState()
    {
        // Evitar que una recolección anterior continúe.
        StopAllCoroutines();

        isGathering = false;
        returningToPool = true;

        // Restaurar el NavMeshAgent por si la tropa fue devuelta mientras estaba recolectando.
        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.isStopped = false;

            if (agent.isOnNavMesh)
                agent.ResetPath();
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        Collider2D troopCollider = GetComponent<Collider2D>();

        if (troopCollider != null)
            troopCollider.enabled = true;

        allyBaseTransform = null;
        gatherPointTransform = null;
        definition = null;
    }

    public void ReturnToPool()
    {
        // Impedir que la misma tropa se devuelva varias veces.
        if (returningToPool)
            return;

        returningToPool = true;

        if (poolManager != null)
        {
            poolManager.Despawn(this);
        }
        else
        {
            // Respaldo por si la tropa no tiene gestor asignado.
            Destroy(gameObject);
        }
    }
    #endregion
}