using UnityEngine;

public static class VictoryComposition
{
    public static VictoryFlowController BuildAndInitialize(
        VictoryView victoryView,
        GameplayEventDirector gameplayEventDirector,
        IInputService inputService,
        MonoBehaviour[] gameplayComponentsToToggle,
        PlayerMovement playerMovement,
        PlayerCombatSystem playerCombatSystem,
        PlayerAnimationController playerAnimationController,
        Object context)
    {
        if (victoryView == null)
        {
            Debug.LogWarning("GameplaySceneEntryPoint: VictoryView is not assigned.", context);
            return null;
        }

        if (gameplayEventDirector == null)
        {
            Debug.LogWarning("GameplaySceneEntryPoint: GameplayEventDirector is not assigned.", context);
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

        VictoryFlowController victoryFlowController = new VictoryFlowController(
            victoryView,
            gameplayEventDirector,
            inputService,
            new UnityGamePauseService(),
            new UnityCursorService(),
            new UnitySceneRestartService(),
            componentsToToggle,
            pauseTimeOnVictory: true,
            unlockCursorOnVictory: true);
        victoryFlowController.Initialize();
        return victoryFlowController;
    }
}
