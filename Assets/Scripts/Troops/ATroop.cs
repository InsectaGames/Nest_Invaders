using UnityEngine;
using UnityEngine.AI;

public class ATroop : ASubject<GameEvent<ResourceSlot>>, IPoolable
{
    // Tipo de tropa.
    [SerializeField] private TroopDefinition definition;
    private Faction faction;

    private TroopPoolManager poolManager;
    private bool returningToPool = true;

    // Componentes de la tropa, generales.
    private NavMeshAgent agent;
    private HealthComponent health;
    private SpriteRenderer spriteRenderer;
    private Collider2D troopCollider;

    // Componentes para el comportamiento de las tropas.
    [Header("Comportamientos disponibles")]
    [SerializeField] private MonoBehaviour[] availableBehaviours;

    private ITroop troopBehaviour;
    private MonoBehaviour troopBehaviourComponent;

    private static GameObject[] bases;

    private Transform allyBaseTransform;

    public TroopDefinition GetDefinition() => definition;
    public Faction GetFaction() => faction;
    public HealthComponent GetHealth() => health;
    public NavMeshAgent GetAgent() => agent;
    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
    public Collider2D GetTroopCollider() => troopCollider;
    public Transform GetAllyBaseTransform() => allyBaseTransform;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<HealthComponent>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        troopCollider = GetComponent<Collider2D>();

        if (troopBehaviourComponent != null)
            troopBehaviourComponent.enabled = false;

        bases = GameObject.FindGameObjectsWithTag("Base");

        // Debug.Log(bases.Length);

        foreach (GameObject obj in bases)
        {
            BaseSystem bs = obj.GetComponent<BaseSystem>();

            if (bs != null && bs.GetFaction() == Faction.ALLY)
            {
                this.AddObserver(bs);
                break;
            }
        }

        // Debug.Log("Se ha girado correctamente");
    }

    private void ActivateTroopBehaviour(TroopType type)
    {
        troopBehaviour = null;
        troopBehaviourComponent = null;

        // Buscar el comportamiento que corresponda al tipo de tropa.
        foreach (MonoBehaviour component in availableBehaviours)
        {
            if (component == null || !(component is ITroop behaviour))
                continue;

            // Desactivar todos los comportamientos encontrados.
            component.enabled = false;

            // Seleccionar únicamente el que corresponda.
            if (troopBehaviour == null && behaviour.Type == type)
            {
                troopBehaviour = behaviour;
                troopBehaviourComponent = component;
            }
        }

        if (troopBehaviour == null)
        {
            Debug.LogError($"{name}: no se ha encontrado un comportamiento para el tipo {type}.");
            return;
        }

        troopBehaviourComponent.enabled = true;
        troopBehaviour.InitializeBehaviour();
    }

    public void Initialize(TroopDefinition troopDefinition, Faction f, TroopPoolManager owner)
    {
        if (troopDefinition == null)
            return;

        definition = troopDefinition;
        faction = f;
        poolManager = owner;

        returningToPool = false;

        // Restaurar la apariencia de la tropa.
        if (spriteRenderer != null)
        {
            if (definition._sprites != null &&
                definition._sprites.Length > 0)
            {
                spriteRenderer.sprite = definition._sprites[0];
            }

            spriteRenderer.color =
                (faction == Faction.ALLY) ? Color.white : Color.red;

            spriteRenderer.enabled = true;
        }

        if (troopCollider != null)
            troopCollider.enabled = true;

        // Configuración común del NavMesh.
        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.speed = definition.speed;
            agent.isStopped = false;
            agent.updateUpAxis = false;

            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning($"{name}: el agente no está situado sobre el NavMesh.");
                return;
            }
            
            agent.ResetPath();
        }

        allyBaseTransform = FindBaseTransform(f);

        // Inicializar el comportamiento correspondiente al tipo de tropa.
        ActivateTroopBehaviour(definition.type);

        // Restablecer la vida para esta nueva utilización.
        if (health != null)
            health.Initialize(definition.maxHealth);
    }

    // Busca una base perteneciente a la facción indicada.
    public Transform FindBaseTransform(Faction f)
    {
        if (bases == null)
            return null;

        foreach (GameObject _base in bases)
        {
            if (_base == null)
                continue;

            BaseSystem baseSystem = _base.GetComponent<BaseSystem>();

            if (baseSystem != null && baseSystem.GetFaction() == f)
                return _base.transform;
        }

        return null;
    }

    public void SetTarget(Transform target)
    {
        if (target == null || agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: el agente no está situado sobre el NavMesh.");
            return;
        }

        // Buscar un punto válido del NavMesh cerca del objetivo.
        if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 10f, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning(
                $"{name}: no se ha encontrado un punto válido del NavMesh " +
                $"cerca de {target.name}."
            );
        }
    }

    // Cuando entra en el trigger de una base.
    // Es un comportamiento común a todas las tropas.
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Base"))
            return;

        GameObject baseObj = col.gameObject;

        BaseSystem baseCol = baseObj.GetComponent<BaseSystem>();
        HealthComponent baseHealth = baseObj.GetComponent<HealthComponent>();

        if (baseCol == null || baseHealth == null)
            return;

        // Si llega a una base enemiga, le hace daño y vuelve al pool.
        if (baseCol.GetFaction() != faction)
        {
            baseHealth.TakeDamage();
            ReturnToPool();
        }
    }

    // Comunicar recursos a los observadores de la tropa.
    public void SendResourceEvent(LogicEvent logicEvent, ResourceSlot resourceSlot) =>
        UpdateObservers(new GameEvent<ResourceSlot>(logicEvent, resourceSlot));

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
        // Restaurar los comportamientos específicos.
        // Restablecer el comportamiento específico.
        if (troopBehaviour != null)
            troopBehaviour.ResetBehaviour();

        if (troopBehaviourComponent != null)
            troopBehaviourComponent.enabled = false;

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

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (troopCollider != null)
            troopCollider.enabled = true;

        allyBaseTransform = null;
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
