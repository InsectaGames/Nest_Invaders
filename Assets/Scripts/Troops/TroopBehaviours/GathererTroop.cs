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

    private SpriteRenderer spr;
    private NavMeshAgent nvm;

    // Debug para ver radio en el que ve a los enemigos.
    [Header("Detección de tropas")]
    [SerializeField] private float detectionRadius = 8.5f;
    [SerializeField] private bool showDetectionRadius = true;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
    }

    private void Update()
    {
        // UpdateSpriteVisuals();
        // OnDrawGizmos();
    }

    #region BEHAVIOUR TREE (API)

    // Hay aliados en el area de vision de la tropa.

    public void InitB()
    {
        
    }
    public void EndB()
    {

    }
    public Status AreAlliesNearby()
    {
        Debug.LogWarning("Entro a comprobar enemigos");
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Debug.Log("Tamaño del array del collider: " + detectedObjects.Length);

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null) continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();

            // Ignorar la propia tropa.
            if (otherTroop == null || otherTroop == troop) continue;

            // Comprobar que sea un aliado.
            if (otherTroop.CompareTag("Ally") || otherTroop.gameObject.CompareTag("Ally"))
            {
                HealthComponent health = otherTroop.GetHealth();

                if (health != null && !health.IsDead())
                    return Status.Success;
            }
        }
        //StartCoroutine(WaitOneSecond());
        return Status.Failure;
    }

    // Hay enemigos en el area de visión de la tropa.
    public Status AreEnemiesNearby()
    {
        Debug.LogWarning("Entro a comprobar enemigos");
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Debug.Log("Tamaño del array del collider: " + detectedObjects.Length);


        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null)
                continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();

            //StartCoroutine(WaitOneSecond());
            //Ignorarse a si mismo
            if (otherTroop == null || otherTroop == troop) continue;

            //Comprobar que sea un enemigo
            if (otherTroop.CompareTag("Enemy") || otherTroop.gameObject.CompareTag("Enemy"))
            {
                TroopDefinition otherTroopDefinition = otherTroop.GetDefinition();

                // Ignorar ladrones o tropas sin definición. Luego habría que ver que no ignore a ladrones descamuflados, de momento lo dejamos así.
                if (otherTroopDefinition == null || otherTroopDefinition.type == TroopType.THIEF) continue;

                HealthComponent health = otherTroop.GetHealth();


                if (health != null && !health.IsDead())
                    return Status.Success;
            }
        }
        return Status.Failure;
    }

    // Compara si hay mayor o igual numero de aliados cercanos que de enemigos.
    public Status CompareAlliesToEnemies()
    {
        Debug.LogWarning("Entro a comprobar entidades en el radio");
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Debug.Log("Tamaño del array del collider: " + detectedObjects.Length);

        int alliesCount = 0;
        int enemiesCount = 0;

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null) continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();
            if (otherTroop == null || otherTroop == troop) continue;

            HealthComponent health = otherTroop.GetHealth();
            if (health == null || health.IsDead()) continue;

            // Contar aliados.
            if (otherTroop.gameObject.CompareTag("Ally"))
                alliesCount++;

            // Contar enemigos, ignorando a los ladrones, luego se mirará si estan invisibles o no para contarlos.
            else if (otherTroop.gameObject.CompareTag("Enemy"))
            {
                TroopDefinition definition = otherTroop.GetDefinition();

                if (definition == null || definition.type == TroopType.THIEF)
                    continue;

                enemiesCount++;
            }
        }

        //StartCoroutine(WaitOneSecond());
        if (alliesCount >= enemiesCount)
            return Status.Success;

        return Status.Failure;
    }

    // 1. Calcular el punto de interés más adecuado.
    public Status CalculateInterestPoint()
    {
        return Status.Failure;
    }

    // 2. Comprobar si existe un punto de interés válido.
    public Status InterestPointExists()
    {
        return Status.Failure;
    }

    // 3. Aumentar la velocidad de movimiento de la tropa
    //    estableciendo un límite máximo.
    public Status IncreaseMovementSpeed()
    {
        return Status.Failure;
    }

    // 4. Desplazarse hacia un objetivo evitando al enemigo
    //    más cercano mediante un rodeo.
    public Status MoveTowardsTargetAvoidingEnemy()
    {
        return Status.Failure;
    }

    // 5. Desplazarse hacia el punto de interés utilizando NavMesh.
    public Status MoveTowardsInterestPoint()
    {
        return Status.Failure;
    }

    // 6. Comprobar si se ha llegado al punto de interés.
    public Status HasReachedInterestPoint()
    {
        return Status.Failure;
    }

    // 7. Ejecutar la acción correspondiente al punto de interés:
    // recoger materiales, depositarlos en la base, etc.
    public Status PerformInterestPointAction()
    {
        return Status.Failure;
    }

    // 8. Comprobar si la tropa está en la base aliada
    // o en una cueva de recolección.
    public Status IsAtBaseOrCollectionCave()
    {
        return Status.Failure;
    }


    // Solo para debbugear y que no pase todo rapidisimo.
    IEnumerator WaitOneSecond()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("Ha pasado un segundo");
    }

    #endregion

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
            troop.SetTarget(gatherPointTransform);
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

            if (isGathering &&
                baseCol.GetFaction() == troop.GetFaction())
            {
                troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.LEAVES, 1));

                Debug.Log(
                    "Recursos entregados en la base. Volviendo a la mina..."
                );

                isGathering = false;

                if (gatherPointTransform != null)
                {
                    troop.SetTarget(gatherPointTransform);
                }
            }
        }
    }

    private IEnumerator GatherRoutine()
    {
        isGathering = true;

        if (troop.GetAgent() != null)
        {
            troop.GetAgent().isStopped = true;
            troop.GetAgent().enabled = false;
        }

        SpriteRenderer spr = troop.GetSpriteRenderer();
        Collider2D collider = troop.GetTroopCollider();

        // Entra en la cueva.
        if (spr != null)
            spr.enabled = false;

        if (collider != null)
            collider.enabled = false;

        yield return new WaitForSeconds(5f);

        if (troop.GetAgent() != null)
        {
            troop.GetAgent().enabled = true;
            troop.GetAgent().isStopped = false;

            if (spr != null)
                spr.enabled = true;

            if (collider != null)
                collider.enabled = true;

            if (troop.GetAllyBaseTransform() != null)
            {
                troop.SetTarget(troop.GetAllyBaseTransform());
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

    private void OnDrawGizmos()
    {
        if (!showDetectionRadius) return;

        // Cambia el color dependiendo de si está recolectando o moviéndose
        if (isGathering)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // Naranja semi-transparente en la cueva
        }
        else
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Verde semi-transparente en movimiento
        }

        // Dibuja el radio utilizando la variable original de tu script
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    #endregion
}