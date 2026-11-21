using UnityEngine;
using UnityEngine.AI;

public class UpdateVisuals : MonoBehaviour
{
    private Faction fact;
    private TroopType def;
    private SpriteRenderer spr;
    private NavMeshAgent nvm;

    [SerializeField]private Canvas lifeCanvas;
    private Quaternion initialCanvasRotation;

   
    private Vector3 offset = new Vector3(-0.041f, 0.752f, 0f);


    void Start()
    {
        initialCanvasRotation = lifeCanvas.transform.rotation;
        spr = this.GetComponent<SpriteRenderer>();
        def = this.GetComponent<ATroop>().GetDefinition().type;
        fact = this.GetComponent<ATroop>().GetFaction();
        nvm = this.GetComponent<NavMeshAgent>();    
    }

    // Update is called once per frame
    void Update()
    {

        LifeCanvasOffset();

        switch (fact)
        {

            case Faction.ALLY:

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
                }
                break;


            case Faction.ENEMY:
                switch (def)
                {

                    case TroopType.THIEF:
                        RotateSprite(-90f);
                        break;


                    case TroopType.MELEE_ATTACKER:
                        RotateSprite();
                        break;

                    case TroopType.TANK:
                        RotateSprite();
                        break;

               
                }
                break;


            case Faction.None:
                if (def == TroopType.NIGHT_CREATURE)
                    RotateSprite();
            break;

        }
    }


    void FlipSprite34()
    {

        if (Mathf.Abs(nvm.velocity.x) > 0.01f)
        {
            spr.flipX = nvm.velocity.x < 0;
        }
    }


    void RotateSprite(float rotationOffset = 0f)
    {
        lifeCanvas.transform.rotation = initialCanvasRotation;

        // Evitar rotaciones cuando el agente está parado.
        Vector3 velocity = nvm.velocity;

        if (velocity.sqrMagnitude < 0.01f)
            return;

        // Calcular el ángulo de movimiento en el plano XY.
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        // Aplicar corrección de orientación.
        angle += rotationOffset;

        spr.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }


    void LifeCanvasOffset()
    {
      
        lifeCanvas.transform.position = this.transform.position + offset;
    }
}
