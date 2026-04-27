using UnityEngine;

public sealed class PlayerMovementModel
{
    private readonly float moveSpeed;
    private readonly float sprintMultiplier;
    private readonly bool rotateToMovement;
    private readonly float rotationSpeed;
    private readonly float jumpHeight;
    private readonly float gravity;

    private bool wasMoving;
    private bool wasSprinting;

    public PlayerMovementModel(
        float moveSpeed,
        float sprintMultiplier,
        bool rotateToMovement,
        float rotationSpeed,
        float jumpHeight,
        float gravity)
    {
        this.moveSpeed = moveSpeed;
        this.sprintMultiplier = sprintMultiplier;
        this.rotateToMovement = rotateToMovement;
        this.rotationSpeed = rotationSpeed;
        this.jumpHeight = jumpHeight;
        this.gravity = gravity;
    }

    public Vector3 GetMoveDirection(Vector2 input, Vector3 forward, Vector3 right)
    {
        Vector3 planarForward = forward;
        Vector3 planarRight = right;
        planarForward.y = 0f;
        planarRight.y = 0f;
        planarForward.Normalize();
        planarRight.Normalize();

        return (planarForward * input.y + planarRight * input.x).normalized;
    }

    public float GetHorizontalSpeed(bool isSprinting)
    {
        return moveSpeed * (isSprinting ? sprintMultiplier : 1f);
    }

    public float UpdateVerticalVelocity(float currentVerticalVelocity, bool isGrounded, bool jumpPressed, float deltaTime)
    {
        float verticalVelocity = currentVerticalVelocity;

        if (isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * deltaTime;
        return verticalVelocity;
    }

    public Vector3 BuildVelocity(Vector3 moveDirection, float horizontalSpeed, float verticalVelocity)
    {
        Vector3 horizontalVelocity = moveDirection * horizontalSpeed;
        return horizontalVelocity + Vector3.up * verticalVelocity;
    }

    public Vector3 GetLookDirection(Vector3 cameraForward, Vector3 fallbackForward)
    {
        Vector3 look = cameraForward;
        look.y = 0f;

        if (look.sqrMagnitude > 0.0001f)
        {
            return look.normalized;
        }

        return fallbackForward;
    }

    public bool TryBuildRotation(Vector3 lookDirection, Quaternion currentRotation, float deltaTime, out Quaternion newRotation)
    {
        newRotation = currentRotation;
        if (!rotateToMovement || lookDirection.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        newRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * deltaTime);
        return true;
    }

    public bool ShouldLogMovementTransition(Vector3 moveDirection, bool isSprinting)
    {
        bool isMoving = moveDirection.sqrMagnitude > 0.0001f;
        bool changed = isMoving != wasMoving || (isMoving && isSprinting != wasSprinting);
        wasMoving = isMoving;
        wasSprinting = isSprinting;
        return changed;
    }
}
