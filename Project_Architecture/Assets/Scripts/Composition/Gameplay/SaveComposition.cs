using UnityEngine;
using System.Collections.Generic;

using IServiceLocator = ProjectArchitecture.Composition.IServiceLocator;

public static class SaveComposition
{
    public static IGameSaveInteractor BuildAndRegister(
        GameServices services,
        Transform playerTransform,
        HealthComponent playerHealth,
        ManaComponent playerMana,
        PlayerStatsComponent playerStats,
        GameplayEventDirector gameplayEventDirector,
        Object context)
    {
        EnemyStateHandle[] enemyHandles = BuildEnemyHandles();

        IPlayerStateRepository playerRepository = new PlayerStateRepository(
            playerTransform,
            playerHealth,
            playerMana,
            playerStats);
        IEnemyStateRepository enemyRepository = new EnemyStateRepository(enemyHandles);
        IGameplayProgressRepository progressRepository = new GameplayProgressRepository(gameplayEventDirector);

        IGameSaveInteractor gameSaveInteractor = new GameSaveInteractor(
            services.SaveGameRepository,
            playerRepository,
            enemyRepository,
            progressRepository,
            services.GameSessionState,
            services.SceneLoader);

        IServiceLocator locator = services.Locator;
        if (!locator.TryRegister<IGameSaveInteractor>(gameSaveInteractor))
        {
            Debug.LogWarning("GameplaySceneEntryPoint: IGameSaveInteractor is already registered for this scene.", context);
        }

        return gameSaveInteractor;
    }

    private static EnemyStateHandle[] BuildEnemyHandles()
    {
        EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        BossController[] bosses = Object.FindObjectsByType<BossController>(FindObjectsSortMode.None);

        int total = (enemies != null ? enemies.Length : 0) + (bosses != null ? bosses.Length : 0);
        if (total == 0)
        {
            return System.Array.Empty<EnemyStateHandle>();
        }

        List<EnemyStateHandle> handles = new List<EnemyStateHandle>(total);

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    handles.Add(new EnemyStateHandle(enemies[i]));
                }
            }
        }

        if (bosses != null)
        {
            for (int i = 0; i < bosses.Length; i++)
            {
                if (bosses[i] != null)
                {
                    handles.Add(new EnemyStateHandle(bosses[i]));
                }
            }
        }

        return handles.ToArray();
    }
}

