using UnityEngine;
using System.Collections;

public class GathererTroop : MonoBehaviour, ITroop
{
    private ATroop troop;
    public TroopType Type => TroopType.GATHERER;

    private bool isGathering = false;

    private Transform gatherPointTransform;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
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
                troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_1, new ResourceSlot(Resource.FUNGUS, 1));

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

    public void ResetBehaviour()
    {
        // Evitar que una recolección anterior continúe.
        StopAllCoroutines();

        isGathering = false;
        gatherPointTransform = null;
    }
}