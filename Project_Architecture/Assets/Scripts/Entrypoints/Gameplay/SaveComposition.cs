using UnityEngine;

using IServiceLocator = ProjectArchitecture.Composition.IServiceLocator;

public static class SaveComposition
{
    public static IGameSaveInteractor BuildAndRegister(
        GameServices services,
        Transform playerTransform,
        HealthComponent playerHealth,
        ManaComponent playerMana,
        PlayerStatsComponent playerStats,
        Object context)
    {
        EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        IPlayerStateRepository playerRepository = new PlayerStateRepository(
            playerTransform,
            playerHealth,
            playerMana,
            playerStats);
        IEnemyStateRepository enemyRepository = new EnemyStateRepository(enemies);

        IGameSaveInteractor gameSaveInteractor = new GameSaveInteractor(
            services.SaveGameRepository,
            playerRepository,
            enemyRepository,
            services.GameSessionState,
            services.SceneLoader);

        IServiceLocator locator = services.Locator;
        if (!locator.TryRegister<IGameSaveInteractor>(gameSaveInteractor))
        {
            Debug.LogWarning("GameplaySceneEntryPoint: IGameSaveInteractor is already registered for this scene.", context);
        }

        return gameSaveInteractor;
    }
}
