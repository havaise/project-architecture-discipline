using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputService inputService;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool rotateToMovement;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (inputService == null)
        {
            inputService = FindFirstObjectByType<InputService>();
        }
    }

    private void Update()
    {
        if (inputService == null)
        {
            return;
        }

        Vector2 input = inputService.Move;
        Vector3 moveDirection = GetMoveDirection(input);

        Move(moveDirection);
        HandleJump();
        ApplyGravity();

        if (rotateToMovement)
        {
            RotateTowards(moveDirection);
        }
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

    private void Move(Vector3 moveDirection)
    {
        Vector3 horizontalVelocity = moveDirection * moveSpeed;
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
}
