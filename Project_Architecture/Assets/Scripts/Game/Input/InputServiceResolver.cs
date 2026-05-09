using UnityEngine;

public static class InputServiceResolver
{
    public static bool TryResolve(ref IInputService inputService, ref MonoBehaviour inputServiceSource)
    {
        if (inputService != null)
        {
            return true;
        }

        inputService = inputServiceSource as IInputService;
        if (inputService != null)
        {
            return true;
        }

        InputServiceComponent sceneInputService = Object.FindFirstObjectByType<InputServiceComponent>();
        if (sceneInputService == null)
        {
            return false;
        }

        inputServiceSource = sceneInputService;
        inputService = sceneInputService;
        return true;
    }
}
