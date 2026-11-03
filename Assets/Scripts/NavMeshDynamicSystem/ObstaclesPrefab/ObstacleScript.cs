using UnityEditor.SearchService;
using UnityEngine;

public class ObstacleScript : MonoBehaviour, IObserver<Color> // Para ajustarlo en función de la noche.
{
    public Sprite[] obstaclesSprites;
    private SpriteRenderer spr;
    private CircleCollider2D col;

    [SerializeField] private float separation = 0.1f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float poiSafeDistance = 2f;

    private void Awake()
    {
        spr = this.GetComponent<SpriteRenderer>();
        col = this.GetComponent<CircleCollider2D>(); 
        
        int rnd = Random.Range(0, obstaclesSprites.Length);
        spr.sprite = obstaclesSprites[rnd];

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
    }

    private void Start()
    {
        AdjustCollider();  
    }

    private void AdjustCollider() { col.radius = Mathf.Max(spr.sprite.bounds.extents.x, spr.sprite.bounds.extents.y); }

    public bool InitializePos(Vector2 areaMin, Vector2 areaMax, GameObject[] pois)
    {
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y)
            );

            if (col == null)
            {
                return false;
            }

            float radius = col.bounds.extents.x + separation;

            if (radius <= 0f)
            {
                return false;
            }
           
            // 1. Comprobar colisiones con otros obstáculos.
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                candidate,
                col.bounds.extents.x + separation,
                obstacleLayer
            );

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
            foreach (GameObject go in pois)
            {
                Transform poi = go.transform;

                if (poi == null)
                    continue;

                if (Vector2.Distance(candidate, poi.position)
                    < poiSafeDistance + col.bounds.extents.x)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            // 3. La posición cumple todas las condiciones.
            transform.position = new Vector3(candidate.x, candidate.y, 0);

            return true;
        }

        Debug.LogWarning("No se encontró una posición válida para el obstáculo.", this);

        return false;
    }

    public void UpdateObserver(Color data)
    {
        spr.color = data;
    }
}