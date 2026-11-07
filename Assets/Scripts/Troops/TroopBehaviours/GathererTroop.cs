using UnityEngine;
using System.Collections;
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
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private bool showDetectionRadius = true;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
    }

    void Update()
    {
        UpdateSpriteVisuals();    
    }

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

    public void UpdateSpriteVisuals()
    {

        spr.flipX = (nvm.velocity.x < 0);

        spr.flipY = (nvm.velocity.y > 0);

    }
    public void ResetBehaviour()
    {
        // Evitar que una recolección anterior continúe.
        StopAllCoroutines();

        isGathering = false;
        gatherPointTransform = null;
    }

    //Region para debug de mostrar el radio del gizmo
    #region 
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