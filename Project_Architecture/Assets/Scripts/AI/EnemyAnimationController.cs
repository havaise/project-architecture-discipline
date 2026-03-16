using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private Animator animator;

    [Header("Movement Params")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string moveSpeedParam = "MoveSpeed";
    [SerializeField] private float speedDampTime = 0.08f;

    [Header("Attack Params")]
    [SerializeField] private string meleeAttackTriggerParam = "MeleeAttackTrigger";
    [SerializeField] private string rangedAttackTriggerParam = "RangedAttackTrigger";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private int isMovingHash;
    private int moveSpeedHash;
    private int meleeAttackTriggerHash;
    private int rangedAttackTriggerHash;

    private bool callbacksBound;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (enemyAI == null)
        {
            enemyAI = GetComponent<EnemyAI>();
        }

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

        if (enemyAI == null)
        {
            enemyAI = GetComponent<EnemyAI>();
            BindCallbacks();
            return;
        }

        animator.SetBool(isMovingHash, enemyAI.IsMoving);
        animator.SetFloat(moveSpeedHash, enemyAI.MoveSpeedNormalized, speedDampTime, Time.deltaTime);
    }

    private void BindCallbacks()
    {
        if (callbacksBound || enemyAI == null)
        {
            return;
        }

        enemyAI.MeleeAttackPerformed += OnMeleeAttackPerformed;
        enemyAI.RangedAttackPerformed += OnRangedAttackPerformed;
        callbacksBound = true;
    }

    private void UnbindCallbacks()
    {
        if (!callbacksBound || enemyAI == null)
        {
            return;
        }

        enemyAI.MeleeAttackPerformed -= OnMeleeAttackPerformed;
        enemyAI.RangedAttackPerformed -= OnRangedAttackPerformed;
        callbacksBound = false;
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
