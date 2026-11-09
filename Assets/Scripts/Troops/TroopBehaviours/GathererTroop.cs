using BehaviourAPI.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GathererTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    public TroopType Type => TroopType.GATHERER;

    private bool isGathering = false;

    private Transform gatherPointTransform;
    private Transform currentTarget;

    private SpriteRenderer spr;
    private NavMeshAgent nvm;

    // Debug para ver radio en el que ve a los enemigos.
    /*
    [Header("Detección de tropas")]
    [SerializeField] private float detectionRadius = 8.5f;
    [SerializeField] private bool showDetectionRadius = true;
    */

    private void Awake()
    {
        troop = GetComponent<ATroop>();
    }

    private void Update()
    {
        // UpdateSpriteVisuals();

        if (troop == null || troop.GetDefinition() == null)
            return;

        if (isGathering)
            return;

        NavMeshAgent agent = troop.GetAgent();

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (currentTarget == null || agent.pathPending)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // Si ya hemos llegado, no necesitamos recuperar el destino.
        if (distanceToTarget <= agent.stoppingDistance + 0.2f)
            return;

        // Recuperar el camino si se ha perdido o es inválido.
        if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            troop.SetTarget(currentTarget);
        }
    }

    // Inicializar y resetear el comportamiento al spawnear y volver al pool.
    #region INIT Y RESET

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (troop == null)
            return;

        isGathering = false;

        // Habría que establecer también para que regrese a la base, pille recursos...
        GameObject gatherPoint = GameObject.FindGameObjectWithTag("GatherPoint");

        if (gatherPoint != null)
        {
            gatherPointTransform = gatherPoint.transform;
            currentTarget = gatherPointTransform;

            troop.SetTarget(currentTarget);
        }
        else
        {
            Debug.LogWarning("No se ha encontrado ningún objeto con el tag GatherPoint.");
        }

        spr = this.GetComponent<SpriteRenderer>();
        nvm = this.GetComponent<NavMeshAgent>();
    }

    public void ResetBehaviour()
    {
        // Evitar que una recolección anterior continúe.
        StopAllCoroutines();

        isGathering = false;
        gatherPointTransform = null;
        currentTarget = null;
    }

    #endregion

    // Métodos antiguos para calcular posiciones y demás.
    #region TRIGGER Y GATHER

    // Cuando entra en el trigger del GatherPoint o de la base aliada.
    private void OnTriggerEnter2D(Collider2D col)
    {
        // Detecta cuando el Gatherer llega al GatherPoint por primera vez.
        if (!isGathering && col.CompareTag("GatherPoint"))
        {
            StartCoroutine(GatherRoutine());
            return;
        }

        if (col.CompareTag("Base"))
        {
            BaseSystem baseCol = col.GetComponent<BaseSystem>();

            if (baseCol == null)
                return;

            if (isGathering && baseCol.GetFaction() == troop.GetFaction())
            {
                troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.LEAVES, 1));

                Debug.Log("Recursos entregados en la base. Volviendo a la mina...");

                isGathering = false;

                if (gatherPointTransform != null)
                {
                    currentTarget = gatherPointTransform;
                    troop.SetTarget(currentTarget);
                }
            }
        }
    }

    private IEnumerator GatherRoutine()
    {
        isGathering = true;

        NavMeshAgent agent = troop.GetAgent();

        // Detener el movimiento sin desactivar el agente.
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        SpriteRenderer spr = troop.GetSpriteRenderer();
        Collider2D collider = troop.GetTroopCollider();

        // Ocultar la tropa mientras recolecta.
        if (spr != null)
            spr.enabled = false;

        if (collider != null)
            collider.enabled = false;

        yield return new WaitForSeconds(5f);

        // Restaurar la apariencia.
        if (spr != null)
            spr.enabled = true;

        if (collider != null)
            collider.enabled = true;

        // Reanudar el movimiento.
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;

            // Regresar a la base aliada.
            Transform allyBase = troop.GetAllyBaseTransform();

            if (allyBase != null)
            {
                currentTarget = allyBase;
                troop.SetTarget(currentTarget);
            }
            else
            {
                Debug.LogWarning($"{name}: no se ha encontrado la base aliada.");
            }
        }
    }

    #endregion


    //Metodos auxiliares
    #region MÉTODOS AUXILIARES

    public void UpdateSpriteVisuals()
    {
        spr.flipX = (nvm.velocity.x < 0);
        //spr.flipY = (nvm.velocity.y > 0);
    }
    #endregion
}