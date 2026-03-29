using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputService inputService;
    [SerializeField] private PlayerCombatSystem combatSystem;
    [SerializeField] private Animator animator;

    [Header("Movement Params")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string moveSpeedParam = "MoveSpeed";
    [SerializeField] private float speedDampTime = 0.08f;

    [Header("Attack Params")]
    [SerializeField] private string physicalAttackTriggerParam = "PhysicalAttackTrigger";
    [SerializeField] private string magicAttackTriggerParam = "MagicAttackTrigger";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private int isMovingHash;
    private int moveSpeedHash;
    private int physicalAttackTriggerHash;
    private int magicAttackTriggerHash;

    private bool callbacksBound;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (inputService == null)
        {
            inputService = FindFirstObjectByType<InputService>();
        }

        if (combatSystem == null)
        {
            combatSystem = GetComponent<PlayerCombatSystem>();
        }

        if (combatSystem == null)
        {
            combatSystem = FindFirstObjectByType<PlayerCombatSystem>();
        }

        CacheParameterHashes();
    }

    private void OnEnable()
    {
        BindCombatCallbacks();
    }

    private void OnDisable()
    {
        UnbindCombatCallbacks();
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        if (inputService == null)
        {
            inputService = FindFirstObjectByType<InputService>();
            return;
        }

        if (combatSystem == null)
        {
            combatSystem = GetComponent<PlayerCombatSystem>();
            if (combatSystem == null)
            {
                combatSystem = FindFirstObjectByType<PlayerCombatSystem>();
            }

            BindCombatCallbacks();
        }

        Vector2 moveInput = inputService.Move;
        float moveMagnitude = Mathf.Clamp01(moveInput.magnitude);
        bool isMoving = moveMagnitude > 0.01f;

        animator.SetBool(isMovingHash, isMoving);
        animator.SetFloat(moveSpeedHash, moveMagnitude, speedDampTime, Time.deltaTime);
    }

    private void BindCombatCallbacks()
    {
        if (callbacksBound || combatSystem == null)
        {
            return;
        }

        combatSystem.PhysicalAttackPerformed += OnPhysicalAttackPerformed;
        combatSystem.MagicAttackPerformed += OnMagicAttackPerformed;
        callbacksBound = true;
    }

    private void UnbindCombatCallbacks()
    {
        if (!callbacksBound || combatSystem == null)
        {
            return;
        }

        combatSystem.PhysicalAttackPerformed -= OnPhysicalAttackPerformed;
        combatSystem.MagicAttackPerformed -= OnMagicAttackPerformed;
        callbacksBound = false;
    }

    private void OnPhysicalAttackPerformed()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(magicAttackTriggerHash);
        animator.SetTrigger(physicalAttackTriggerHash);
        Log("Physical attack animation triggered.");
    }

    private void OnMagicAttackPerformed()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(physicalAttackTriggerHash);
        animator.SetTrigger(magicAttackTriggerHash);
        Log("Magic attack animation triggered.");
    }

    private void CacheParameterHashes()
    {
        isMovingHash = Animator.StringToHash(isMovingParam);
        moveSpeedHash = Animator.StringToHash(moveSpeedParam);
        physicalAttackTriggerHash = Animator.StringToHash(physicalAttackTriggerParam);
        magicAttackTriggerHash = Animator.StringToHash(magicAttackTriggerParam);
    }

    private void Log(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log($"[PlayerAnimationController] {message}", this);
    }
}
