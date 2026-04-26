using UnityEngine;

public static class PauseMenuComposition
{
    public static PauseMenuController BuildAndInitialize(
        PauseMenuView pauseMenuView,
        IInputService inputService,
        IGameSaveInteractor gameSaveInteractor,
        ISceneLoader sceneLoader,
        MonoBehaviour[] gameplayComponentsToToggle,
        PlayerMovement playerMovement,
        PlayerCombatSystem playerCombatSystem,
        PlayerAnimationController playerAnimationController,
        string mainMenuSceneName,
        Object context)
    {
        if (pauseMenuView == null)
        {
            Debug.LogError("GameplaySceneEntryPoint: PauseMenuView is not assigned.", context);
            return null;
        }

        MonoBehaviour[] componentsToToggle = gameplayComponentsToToggle;
        if (componentsToToggle == null || componentsToToggle.Length == 0)
        {
            componentsToToggle = new MonoBehaviour[]
            {
                playerMovement,
                playerCombatSystem,
                playerAnimationController
            };
        }

        PauseMenuController pauseMenuController = new PauseMenuController(
            pauseMenuView,
            inputService,
            gameSaveInteractor,
            sceneLoader,
            componentsToToggle,
            mainMenuSceneName);
        pauseMenuController.Initialize();
        return pauseMenuController;
    }
}
