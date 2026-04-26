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
        EnemyStateHandle[] enemyHandles = BuildEnemyHandles(enemies);

        IPlayerStateRepository playerRepository = new PlayerStateRepository(
            playerTransform,
            playerHealth,
            playerMana,
            playerStats);
        IEnemyStateRepository enemyRepository = new EnemyStateRepository(enemyHandles);

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

    private static EnemyStateHandle[] BuildEnemyHandles(EnemyController[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            return System.Array.Empty<EnemyStateHandle>();
        }

        EnemyStateHandle[] handles = new EnemyStateHandle[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            handles[i] = new EnemyStateHandle(enemies[i]);
        }

        return handles;
    }
}
