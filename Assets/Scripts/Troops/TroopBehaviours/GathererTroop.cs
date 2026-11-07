using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
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

    //Debug para ver radio en el que ve a los enemigos
    [Header("Detección de tropas")]
    [SerializeField] private float detectionRadius = 8.5f;
    [SerializeField] private bool showDetectionRadius = true;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
    }

    void Update()
    {
        //UpdateSpriteVisuals();
        //OnDrawGizmos();
    }

    //Metodos que usa el Behaviour Tree de la API
    #region

    //Hay aliados en el area de vision de la tropa
    public Status AreAlliesNearby()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null) continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();

            //Ignorar la propia tropa.
            if (otherTroop == null || otherTroop == troop) continue;

            //Comprobar que sea un aliado.
            if (otherTroop.CompareTag("Ally") || otherTroop.gameObject.CompareTag("Ally"))
            {
                HealthComponent health = otherTroop.GetHealth();

                if (health != null && !health.IsDead())
                    return Status.Success;
            }
        }

        return Status.Failure;
    }

    //Hay enemigos en el area de visión de la tropa
    public Status AreEnemiesNearby()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D collider in detectedObjects)
        {
            if (collider == null)
                continue;

            ATroop otherTroop = collider.GetComponentInParent<ATroop>();


            //Ignorarse a si mismo
            if (otherTroop == null || otherTroop == troop) continue;

            //Comprobar que sea un enemigo
            if (otherTroop.gameObject.CompareTag("Enemy") || otherTroop.gameObject.CompareTag("Enemy"))
            {
                TroopDefinition otherTroopDefinition = otherTroop.GetDefinition();

                //Ignorar ladrones o tropas sin definición. Luego habria que ver que no ignore a ladrones descamuflados, de momento lo dejamos asi
                if (otherTroopDefinition == null || otherTroopDefinition.type == TroopType.THIEF) continue;

                HealthComponent health = otherTroop.GetHealth();

                if (health != null && !health.IsDead())
                    return Status.Success;
            }
        }
        return Status.Failure;
    }

    //Compara si hay mayor o igual numero de aliados cercanos que de enemigos
    public Status ComparteAlliesToEnemies()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

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

            // Contar enemigos, ignorando a los ladrones, luego se mirara si estan invisibles o no para contarlos.
            else if (otherTroop.gameObject.CompareTag("Enemy"))
            {
                TroopDefinition definition = otherTroop.GetDefinition();

                if (definition == null || definition.type == TroopType.THIEF)
                    continue;

                enemiesCount++;
            }
        }
        if (alliesCount >= enemiesCount)
            return Status.Success;

        return Status.Failure;
    }


    #endregion


    
    //Inicialisar y resetear el comportamiento al spawnear y volver al pool
    #region
    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (troop == null)
            return;

        isGathering = false;

        // Habría que establecer también para que regrese a la base, pille recursos...
        GameObject gatherPoint =
            GameObject.FindGameObjectWithTag("GatherPoint");

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


    
    //Metodos antiguos para calcular posiciones y demas
    #region

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
    #region 
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