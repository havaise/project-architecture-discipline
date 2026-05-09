using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private static readonly int UnknownBossState = -1;

    [Header("References")]
    [SerializeField] private MonoBehaviour enemyStateSource;
    [SerializeField] private MonoBehaviour enemyAttackEventsSource;
    [SerializeField] private Animator animator;

    [Header("Movement Params")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string moveSpeedParam = "MoveSpeed";
    [SerializeField] private float speedDampTime = 0.08f;

    [Header("Attack Params")]
    [SerializeField] private string meleeAttackTriggerParam = "MeleeAttackTrigger";
    [SerializeField] private string rangedAttackTriggerParam = "RangedAttackTrigger";
    [SerializeField] private string bossStateParam = "BossState";
    [SerializeField] private string hitTriggerParam = "HitTrigger";
    [SerializeField] private string deathTriggerParam = "DeathTrigger";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private int isMovingHash;
    private int moveSpeedHash;
    private int meleeAttackTriggerHash;
    private int rangedAttackTriggerHash;
    private int bossStateHash;
    private int hitTriggerHash;
    private int deathTriggerHash;

    private IMovementStateProvider enemyStateProvider;
    private IStateNameProvider stateNameProvider;
    private IEnemyAttackEvents enemyAttackEvents;
    private IHealth health;
    private bool callbacksBound;
    private int lastBossState = UnknownBossState;
    private bool wasDead;
    private int lastKnownHealth = int.MaxValue;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        ResolveDependencies();

        CacheParameterHashes();
    }

    private void OnEnable()
    {
        BindCallbacks();
        BindHealthCallbacks();
    }

    private void OnDisable()
    {
        UnbindCallbacks();
        UnbindHealthCallbacks();
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        if (!ResolveDependencies())
        {
            return;
        }

        if (!wasDead)
        {
            animator.SetBool(isMovingHash, enemyStateProvider.IsMoving);
            animator.SetFloat(moveSpeedHash, enemyStateProvider.MoveSpeedNormalized, speedDampTime, Time.deltaTime);
        }

        if (stateNameProvider != null && bossStateHash != 0)
        {
            int mappedState = MapStateNameToBossState(stateNameProvider.CurrentStateName);
            if (mappedState != lastBossState)
            {
                animator.SetInteger(bossStateHash, mappedState);
                lastBossState = mappedState;
            }
        }
    }

    private void BindCallbacks()
    {
        if (callbacksBound || enemyAttackEvents == null)
        {
            return;
        }

        enemyAttackEvents.MeleeAttackPerformed += OnMeleeAttackPerformed;
        enemyAttackEvents.RangedAttackPerformed += OnRangedAttackPerformed;
        callbacksBound = true;
    }

    private void UnbindCallbacks()
    {
        if (!callbacksBound || enemyAttackEvents == null)
        {
            return;
        }

        enemyAttackEvents.MeleeAttackPerformed -= OnMeleeAttackPerformed;
        enemyAttackEvents.RangedAttackPerformed -= OnRangedAttackPerformed;
        callbacksBound = false;
    }

    private bool ResolveDependencies()
    {
        if (enemyStateProvider == null)
        {
            enemyStateProvider = enemyStateSource as IMovementStateProvider;
        }

        if (enemyStateProvider == null && enemyAttackEventsSource != null)
        {
            enemyStateProvider = enemyAttackEventsSource as IMovementStateProvider;
        }

        if (enemyStateProvider == null)
        {
            enemyStateProvider = GetComponent<IMovementStateProvider>();
        }

        if (stateNameProvider == null)
        {
            stateNameProvider = enemyStateSource as IStateNameProvider;
        }

        if (stateNameProvider == null && enemyAttackEventsSource != null)
        {
            stateNameProvider = enemyAttackEventsSource as IStateNameProvider;
        }

        if (stateNameProvider == null)
        {
            stateNameProvider = GetComponent<IStateNameProvider>();
        }

        if (enemyAttackEvents == null)
        {
            enemyAttackEvents = enemyAttackEventsSource as IEnemyAttackEvents;
        }

        if (enemyAttackEvents == null && enemyStateSource != null)
        {
            enemyAttackEvents = enemyStateSource as IEnemyAttackEvents;
        }

        if (enemyAttackEvents == null)
        {
            enemyAttackEvents = GetComponent<IEnemyAttackEvents>();
        }

        if (health == null)
        {
            health = GetComponentInChildren<IHealth>();
            if (health != null && lastKnownHealth == int.MaxValue)
            {
                lastKnownHealth = health.Current;
                wasDead = health.IsDead();
            }
        }

        BindCallbacks();
        BindHealthCallbacks();
        return enemyStateProvider != null && enemyAttackEvents != null;
    }

    private void OnMeleeAttackPerformed()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(rangedAttackTriggerHash);
        animator.SetTrigger(meleeAttackTriggerHash);
        Log("Melee attack animation triggered.");
    }

    private void OnRangedAttackPerformed()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(meleeAttackTriggerHash);
        animator.SetTrigger(rangedAttackTriggerHash);
        Log("Ranged attack animation triggered.");
    }

    private void CacheParameterHashes()
    {
        isMovingHash = Animator.StringToHash(isMovingParam);
        moveSpeedHash = Animator.StringToHash(moveSpeedParam);
        meleeAttackTriggerHash = Animator.StringToHash(meleeAttackTriggerParam);
        rangedAttackTriggerHash = Animator.StringToHash(rangedAttackTriggerParam);
        hitTriggerHash = string.IsNullOrWhiteSpace(hitTriggerParam) ? 0 : Animator.StringToHash(hitTriggerParam);
        deathTriggerHash = string.IsNullOrWhiteSpace(deathTriggerParam) ? 0 : Animator.StringToHash(deathTriggerParam);
        bossStateHash = string.IsNullOrWhiteSpace(bossStateParam)
            ? 0
            : Animator.StringToHash(bossStateParam);
    }

    private static int MapStateNameToBossState(string stateName)
    {
        switch (stateName)
        {
            case "Aggression":
                return 1;
            case "Search":
                return 2;
            case "Attack":
                return 3;
            case "StrongAttack":
                return 4;
            case "Dodge":
                return 5;
            case "Recover":
            case "Flee":
                return 6;
            case "Rest":
            default:
                return 0;
        }
    }

    private void BindHealthCallbacks()
    {
        if (health == null)
        {
            return;
        }

        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
        health.HealthChanged += OnHealthChanged;
        health.Died += OnDied;
    }

    private void UnbindHealthCallbacks()
    {
        if (health == null)
        {
            return;
        }

        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (wasDead || hitTriggerHash == 0)
        {
            lastKnownHealth = current;
            return;
        }

        if (lastKnownHealth != int.MaxValue && current < lastKnownHealth)
        {
            animator.SetTrigger(hitTriggerHash);
        }

        lastKnownHealth = current;
    }

    private void OnDied()
    {
        wasDead = true;
        if (deathTriggerHash != 0)
        {
            animator.SetTrigger(deathTriggerHash);
        }

        animator.SetBool(isMovingHash, false);
        animator.SetFloat(moveSpeedHash, 0f);
    }

    private void Log(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log($"[EnemyAnimationController] {message}", this);
    }
}

