using UnityEngine;

public class ObstacleScript : MonoBehaviour //, IObserver<Color> // Para ajustarlo en función de la noche.
{
    [SerializeField] private Sprite[] obstaclesSprites;
    private SpriteRenderer spr;
    private CircleCollider2D col;

    [SerializeField] private float separation = 0.1f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float poiSafeDistance = 2f;

    private bool isInitialized = false;

    private void Awake()
    {
        spr = this.GetComponent<SpriteRenderer>();
        col = this.GetComponent<CircleCollider2D>();

        if (spr == null || col == null)
        {
            Debug.LogError("ObstacleScript necesita SpriteRenderer y CircleCollider2D.", this);
            return;
        }

        if (obstaclesSprites == null || obstaclesSprites.Length == 0)
        {
            Debug.LogError("ObstacleScript: No hay sprites de obstáculos asignados.", this);
            return;
        }
        
        int rnd = Random.Range(0, obstaclesSprites.Length);
        spr.sprite = obstaclesSprites[rnd];

        AdjustCollider();
        isInitialized = true;

       /*
        GameObject obj = GameObject.FindGameObjectWithTag("DNSys");
        if(obj != null)
        {
            DayNightSystem dns = obj.GetComponent<DayNightSystem>();

         
            if(dns != null)
               dns.AddObserver(this);
            else
                Debug.LogWarning("El objeto encontrado no tiene asignado un DayNightSystem.");
        }
        else
        {
            Debug.LogWarning("No se ha encontrado ningún objeto con el tag \"DNSys\".");
        }
         */
    }

    private void AdjustCollider()
    {
        if (spr == null || spr.sprite == null || col == null)
            return;

        col.radius = Utils.Max(spr.sprite.bounds.extents.x, spr.sprite.bounds.extents.y);
    }

    public bool InitializePos(Vector2 areaMin, Vector2 areaMax, GameObject[] pois)
    {
        if (!isInitialized || col == null)
            return false;

        int maxAttempts = 100;

        float obstacleRadius = col.bounds.extents.x;
        float placementRadius = obstacleRadius + separation;

        if (obstacleRadius <= 0f)
            return false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(Utils.GetRandomFloat(areaMin.x, areaMax.x), Utils.GetRandomFloat(areaMin.y, areaMax.y));

            // 1. Comprobar colisiones con otros obstáculos.
            Collider2D[] hits = Physics2D.OverlapCircleAll(candidate, placementRadius, obstacleLayer);

            bool valid = true;

            foreach (Collider2D hit in hits)
            {
                if (hit != col)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            // 2. Comprobar distancia a los puntos de interés.
            if (pois != null)
            {
                foreach (GameObject go in pois)
                {
                    if (go == null)
                        continue;

                    if (Vector2.Distance(candidate, go.transform.position) < poiSafeDistance + obstacleRadius)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
                continue;

            // 3. La posición cumple todas las condiciones.
            transform.position = new Vector3(candidate.x, candidate.y, 0f);

            return true;
        }

        Debug.LogWarning("No se encontró una posición válida para el obstáculo.", this);

        return false;
    }

/*
    public void UpdateObserver(Color data)
    {
        spr.color = data;
    }
*/
}