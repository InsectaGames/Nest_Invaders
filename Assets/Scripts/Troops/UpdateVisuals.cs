using UnityEngine;
using UnityEngine.AI;

public class UpdateVisuals : MonoBehaviour
{
    private TroopType def;

    private SpriteRenderer spr;
    private NavMeshAgent nvm;

    [SerializeField] private Canvas lifeCanvas;

    private Quaternion initialCanvasRotation;
    private Quaternion initialSpriteRotation;

    private Vector3 offset = new Vector3(-0.041f, 0.752f, 0f);

    private bool initialized;

    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        nvm = GetComponent<NavMeshAgent>();

        if (lifeCanvas != null)
            initialCanvasRotation = lifeCanvas.transform.rotation;

        if (spr != null)
            initialSpriteRotation = spr.transform.rotation;
    }

    /// <summary>
    /// Se llama cada vez que la tropa se inicializa,
    /// incluso cuando procede del Object Pool.
    /// </summary>
    public void InitializeVisuals(TroopType type)
    {
        def = type;
        initialized = true;

        ResetVisuals();
    }

    private void Update()
    {
        if (!initialized)
            return;

        LifeCanvasOffset();

        if (nvm == null || spr == null)
            return;

        switch (def)
        {
            case TroopType.GATHERER:
                FlipSprite34();
                break;

            case TroopType.MELEE_ATTACKER:
                RotateSprite();
                break;

            case TroopType.TANK:
                FlipSprite34();
                break;

            case TroopType.RANGED_ATTACKER:
                FlipSprite34();
                break;

            case TroopType.THIEF:
                RotateSprite(-90f);
                break;

            case TroopType.ENEMY_MELEE:
                RotateSprite(-180f);
                break;

            case TroopType.ENEMY_TANK:
                RotateSprite();
                break;

            case TroopType.NIGHT_CREATURE:
                RotateSprite(-180f);
                break;
        }
    }

    private void FlipSprite34()
    {
        if (nvm.destination.x > 0.01f)
        {
            spr.flipX = false;
        }
        else if (nvm.destination.x < -0.01f)
        {
            spr.flipX = true;
        }
    }

    private void RotateSprite(float rotationOffset = 0f)
    {
        if (lifeCanvas != null)
            lifeCanvas.transform.rotation = initialCanvasRotation;

        Vector3 velocity = nvm.velocity;

        // Si está parado, conserva la última orientación.
        if (velocity.sqrMagnitude < 0.01f)
            return;

        // Calcular el ángulo de movimiento en XY.
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        // Aplicar corrección de orientación.
        angle += rotationOffset;

        spr.transform.rotation = initialSpriteRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void LifeCanvasOffset()
    {
        if (lifeCanvas == null)
            return;

        lifeCanvas.transform.position = transform.position + offset;
    }

    /// <summary>
    /// Restablece las visuales antes de reutilizar la tropa.
    /// </summary>
    public void ResetVisuals()
    {
        if (spr != null)
        {
            spr.transform.rotation = initialSpriteRotation;
            spr.flipX = false;
        }

        if (lifeCanvas != null)
        {
            lifeCanvas.transform.rotation = initialCanvasRotation;
            lifeCanvas.transform.position = transform.position + offset;
        }
    }
}