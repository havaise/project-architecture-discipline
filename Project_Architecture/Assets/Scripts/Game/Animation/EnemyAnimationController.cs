using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
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

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private int isMovingHash;
    private int moveSpeedHash;
    private int meleeAttackTriggerHash;
    private int rangedAttackTriggerHash;
    private int bossStateHash;

    private IMovementStateProvider enemyStateProvider;
    private IStateNameProvider stateNameProvider;
    private IEnemyAttackEvents enemyAttackEvents;
    private bool callbacksBound;

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
    }

    private void OnDisable()
    {
        UnbindCallbacks();
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

        animator.SetBool(isMovingHash, enemyStateProvider.IsMoving);
        animator.SetFloat(moveSpeedHash, enemyStateProvider.MoveSpeedNormalized, speedDampTime, Time.deltaTime);

        if (stateNameProvider != null && bossStateHash != 0)
        {
            animator.SetInteger(bossStateHash, MapStateNameToBossState(stateNameProvider.CurrentStateName));
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

        BindCallbacks();
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

    private void Log(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log($"[EnemyAnimationController] {message}", this);
    }
}

