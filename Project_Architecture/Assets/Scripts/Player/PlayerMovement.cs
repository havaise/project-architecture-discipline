using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputService inputServiceSource;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private bool rotateToMovement;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private CharacterController controller;
    private IInputService inputService;
    private float verticalVelocity;
    private bool wasMoving;
    private bool wasSprinting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        ResolveInputService();
    }

    private void Update()
    {
        if (inputService == null)
        {
            ResolveInputService();
            return;
        }

        Vector2 input = inputService.Move;
        Vector3 moveDirection = GetMoveDirection(input);
        bool isSprinting = inputService.IsSprintPressed();
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        Move(moveDirection, currentSpeed);
        HandleJump();
        ApplyGravity();

        if (rotateToMovement)
        {
            RotateTowards(GetLookDirection());
        }

        HandleDebug(moveDirection, isSprinting, currentSpeed);
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 forward;
        Vector3 right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }
        else
        {
            forward = transform.forward;
            right = transform.right;
        }

        return (forward * input.y + right * input.x).normalized;
    }

    private void Move(Vector3 moveDirection, float speed)
    {
        Vector3 horizontalVelocity = moveDirection * speed;
        Vector3 totalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(totalVelocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (!controller.isGrounded)
        {
            return;
        }

        if (verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (inputService.IsJumpPressed())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void RotateTowards(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Vector3 GetLookDirection()
    {
        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;

            if (cameraForward.sqrMagnitude > 0.0001f)
            {
                return cameraForward.normalized;
            }
        }

        return transform.forward;
    }

    private void HandleDebug(Vector3 moveDirection, bool isSprinting, float currentSpeed)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        bool isMoving = moveDirection.sqrMagnitude > 0.0001f;

        if (isMoving != wasMoving || (isMoving && isSprinting != wasSprinting))
        {
            if (isMoving)
            {
                Debug.Log($"[PlayerMovement] Move start. Sprint: {isSprinting}, Speed: {currentSpeed:0.00}, Dir: {moveDirection}", this);
            }
            else
            {
                Debug.Log("[PlayerMovement] Move stop.", this);
            }
        }

        if (inputService.IsPhysicalAttackPressed())
        {
            Debug.Log("[PlayerMovement] LMB pressed -> Physical attack.", this);
        }

        if (inputService.IsMagicAttackPressed())
        {
            Debug.Log("[PlayerMovement] RMB pressed -> Magic attack.", this);
        }

        wasMoving = isMoving;
        wasSprinting = isSprinting;
    }

    public void SetInputService(IInputService service)
    {
        inputService = service;
    }

    private void ResolveInputService()
    {
        if (inputService != null)
        {
            return;
        }

        if (inputServiceSource == null)
        {
            inputServiceSource = FindFirstObjectByType<InputService>();
        }

        inputService = inputServiceSource;
    }
}
