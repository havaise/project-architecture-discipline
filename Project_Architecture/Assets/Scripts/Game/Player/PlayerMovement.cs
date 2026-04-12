using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour inputServiceSource;
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
    private PlayerMovementModel movementModel;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movementModel = new PlayerMovementModel(
            moveSpeed,
            sprintMultiplier,
            rotateToMovement,
            rotationSpeed,
            jumpHeight,
            gravity);

        ResolveInputService();
    }

    private void Update()
    {
        if (!ResolveInputService())
        {
            return;
        }

        Vector2 input = inputService.Move;
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
        Vector3 moveDirection = movementModel.GetMoveDirection(input, forward, right);

        bool isSprinting = inputService.IsSprintPressed();
        float currentSpeed = movementModel.GetHorizontalSpeed(isSprinting);

        verticalVelocity = movementModel.UpdateVerticalVelocity(
            verticalVelocity,
            controller.isGrounded,
            inputService.IsJumpPressed(),
            Time.deltaTime);

        Vector3 totalVelocity = movementModel.BuildVelocity(moveDirection, currentSpeed, verticalVelocity);
        controller.Move(totalVelocity * Time.deltaTime);

        Vector3 lookDirection = movementModel.GetLookDirection(
            cameraTransform != null ? cameraTransform.forward : transform.forward,
            transform.forward);

        if (movementModel.TryBuildRotation(lookDirection, transform.rotation, Time.deltaTime, out Quaternion nextRotation))
        {
            transform.rotation = nextRotation;
        }

        HandleDebug(moveDirection, isSprinting, currentSpeed);
    }

    private void HandleDebug(Vector3 moveDirection, bool isSprinting, float currentSpeed)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        if (movementModel.ShouldLogMovementTransition(moveDirection, isSprinting))
        {
            if (moveDirection.sqrMagnitude > 0.0001f)
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
    }

    private bool ResolveInputService()
    {
        return InputServiceResolver.TryResolve(ref inputService, ref inputServiceSource);
    }
}
