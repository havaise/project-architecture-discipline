using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
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
    private PlayerMovementModel movementModel;
    private PlayerModel lastFrameInput;
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
    }

    public void ProcessFrame(PlayerModel input)
    {
        if (input == null)
        {
            return;
        }

        lastFrameInput = input;

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
        Vector3 moveDirection = movementModel.GetMoveDirection(input.MoveInput, forward, right);

        float currentSpeed = movementModel.GetHorizontalSpeed(input.IsSprinting);

        verticalVelocity = movementModel.UpdateVerticalVelocity(
            verticalVelocity,
            controller.isGrounded,
            input.JumpPressed,
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

        HandleDebug(moveDirection, input.IsSprinting, currentSpeed);
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

        if (lastFrameInput != null && lastFrameInput.PhysicalAttackPressed)
        {
            Debug.Log("[PlayerMovement] LMB pressed -> Physical attack.", this);
        }

        if (lastFrameInput != null && lastFrameInput.MagicAttackPressed)
        {
            Debug.Log("[PlayerMovement] RMB pressed -> Magic attack.", this);
        }
    }
}
