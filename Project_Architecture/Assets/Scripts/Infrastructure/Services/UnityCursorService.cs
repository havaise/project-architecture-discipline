using UnityEngine;

public sealed class UnityCursorService : ICursorService
{
    public void Show()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideAndLock()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
