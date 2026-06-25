using UnityEngine;

public sealed class PlayerModel
{
    public Vector2 MoveInput { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PhysicalAttackPressed { get; private set; }
    public bool MagicAttackPressed { get; private set; }

    public void UpdateFromInput(IInputService inputService)
    {
        if (inputService == null)
        {
            MoveInput = Vector2.zero;
            IsSprinting = false;
            JumpPressed = false;
            PhysicalAttackPressed = false;
            MagicAttackPressed = false;
            return;
        }

        MoveInput = inputService.Move;
        IsSprinting = inputService.IsSprintPressed();
        JumpPressed = inputService.IsJumpPressed();
        PhysicalAttackPressed = inputService.IsPhysicalAttackPressed();
        MagicAttackPressed = inputService.IsMagicAttackPressed();
    }
}
