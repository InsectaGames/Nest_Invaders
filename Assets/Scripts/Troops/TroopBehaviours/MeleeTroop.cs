using UnityEngine;
using UnityEngine.AI;

public class MeleeTroop : MonoBehaviour, ITroop, ICombatState
{
    private ATroop troop;
    public TroopType Type => TroopType.MELEE_ATTACKER;

    [Header("Combate")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float enemyAttackPriorityDelay = 0.2f; // PRIORIDAD DE ATAQUE A ALIADOS.

    public bool IsInCombat => CombatSystem.IsValidEnemy(troop, enemyToAttack);

    [Header("Detección")]
    [SerializeField] private float targetUpdateInterval = 0.2f;

    private ATroop enemyToAttack;

    private float nextAttackTime = 0f;
    private float enemyStartTime;
    private float nextTargetUpdateTime;

    private bool returningToBase;

    private Animator _animator;
    private SpriteRenderer spr;
    private NavMeshAgent nvm;

    private void Awake()
    {
        troop = GetComponent<ATroop>();
        _animator = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        nvm = GetComponent<NavMeshAgent>();
    }

    #region ESTABLECER OBJETIVO

    private void Update()
    {

        UpdateSpriteVisuals();
        if (troop == null || troop.GetDefinition() == null)
            return;

        // Comprobar si el objetivo actual sigue siendo válido.
        if (!HasValidTarget())
        {
            enemyToAttack = null;
            troop.SetMovementStopped(false);
            returningToBase = false;

            // Buscar un nuevo enemigo.
            ATroop closestEnemy = CombatSystem.FindClosestEnemy(troop, troop.GetDefinition().attackReach);

            if (closestEnemy != null)
            {
                SetCombatTarget(closestEnemy, true);
            }
            else
            {
                _animator.SetBool("melee_attacking", false);
                ReturnToEnemyBase();
                return;
            }
        }

        if (enemyToAttack == null)
            return;

        // Comprobar si el enemigo está dentro del alcance.
        if (CombatSystem.CloseEnoughToAttack(troop, enemyToAttack))
        {
            // IMPORTANTE: detener completamente la navegación.
            troop.SetMovementStopped(true);

            // Atacar cuando corresponda.
            if (Time.time >= nextAttackTime && CanAttackNow())
            {
                Attack(enemyToAttack);
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // El enemigo todavía está fuera del alcance.
            troop.SetMovementStopped(false);

            // Actualizar periódicamente su posición como destino.
            if (Time.time >= nextTargetUpdateTime)
            {
                troop.SetTarget(enemyToAttack.transform);

                nextTargetUpdateTime = Time.time + targetUpdateInterval;
            }
        }
    }

    private bool CanAttackNow() => 
        (troop.GetFaction() == Faction.ENEMY && Time.time < enemyStartTime + enemyAttackPriorityDelay) ? false : true;

    public void InitializeBehaviour()
    {
        if (troop == null)
            troop = GetComponent<ATroop>();

        if (troop == null)
            return;

        enemyStartTime = Time.time;

        enemyToAttack = null;
        nextAttackTime = 0f;
        nextTargetUpdateTime = 0f;
        returningToBase = false;

        troop.SetMovementStopped(false);

        ReturnToEnemyBase();
    }

    // Establecer un nuevo objetivo de combate.
    private void SetCombatTarget(ATroop target, bool requestReciprocal)
    {
        if (!CombatSystem.IsValidEnemy(troop, target))
            return;

        _animator.SetBool("melee_attacking", true);

        enemyToAttack = target;
        returningToBase = false;

        nextTargetUpdateTime = Time.time + targetUpdateInterval;

        // Cambiar el destino de movimiento hacia el enemigo.
        troop.SetMovementStopped(false);
        troop.SetTarget(target.transform);

        // Solicitar que el enemigo también nos ataque.
        if (requestReciprocal)
        {
            MeleeTroop enemyBehaviour = target.GetComponent<MeleeTroop>();

            if (enemyBehaviour != null)
            {
                enemyBehaviour.TryAcceptAttacker(troop);
            }
        }
    }

    // Aceptar a una tropa que nos ha seleccionado como objetivo.
    public void TryAcceptAttacker(ATroop attacker)
    {
        if (!CombatSystem.IsValidEnemy(troop, attacker))
            return;

        // No realizar otra solicitud recíproca: evita llamadas circulares entre las dos tropas.
        SetCombatTarget(attacker, false);
        
    }

    // Comprobar si podemos mantener nuestro objetivo actual.
    private bool HasValidTarget() => 
        (enemyToAttack == null) ? false : IsInCombat;

    // Volver a dirigirnos hacia la base enemiga.
    private void ReturnToEnemyBase()
    {
        if (troop == null || returningToBase)
            return;

        Faction enemyFaction = troop.GetFaction() == Faction.ALLY ? Faction.ENEMY : Faction.ALLY;

        Transform enemyBase = troop.FindBaseTransform(enemyFaction);

        if (enemyBase != null)
        {
            returningToBase = true;
            troop.SetTarget(enemyBase);
        }
        else
        {
            Debug.LogWarning($"{name}: no se ha encontrado la base enemiga.");
        }
    }

    #endregion

    // Ejecutar el ataque.
    private void Attack(ATroop target)
    {
        if (!CombatSystem.IsValidEnemy(troop, target))
        { 
            _animator.SetBool("melee_attacking", false);
            return;
        }

        _animator.SetBool("melee_attacking", true);
        CombatSystem.PerformAttack(troop, target);
        
        HealthComponent targetHealth = target.GetHealth();

        if (targetHealth != null && targetHealth.IsDead())
        {
            // Si el enemigo suelta recursos, realizar la tirada.
            if (target.GetFaction() != Faction.ALLY)
            {
                GambleForSticks(target.GetDefinition());
            }

            targetHealth.Die();

            // Liberar el objetivo para buscar otro enemigo.
            enemyToAttack = null;

            // Reanudar movimiento para buscar otro objetivo.
            troop.SetMovementStopped(false);
        }

        _animator.SetBool("melee_attacking", false);
    }

    // Método para conseguir palos. De momento, simplemente será una tirada aleatoria en todos los enemigos.
    // En el futuro, solo se debe aplicar a los bichos que suelten palos.
    private void GambleForSticks(TroopDefinition def)
    {
        if (Utils.GetRandomInclusive(0, 100) > def.chanceToDropSticks)
        {
            troop.SendResourceEvent(LogicEvent.RESOURCE_GATHER_2, new ResourceSlot(Resource.STICK, 1));
        }
    }

    public void UpdateSpriteVisuals()
    {
        spr.flipX = (nvm.velocity.x < 0);
        //spr.flipY = (nvm.velocity.y > 0);
    }
    
    public void ResetBehaviour()
    {
        enemyToAttack = null;
        nextAttackTime = 0f;
        enemyStartTime = 0f;

        returningToBase = false;

        if (_animator != null)
            _animator.SetBool("melee_attacking", false);

        if (troop != null)
            troop.SetMovementStopped(false);
    }
}