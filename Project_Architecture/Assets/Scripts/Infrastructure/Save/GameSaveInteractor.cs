using UnityEngine.SceneManagement;
using System.Collections.Generic;

public sealed class GameSaveInteractor : IGameSaveInteractor
{
    private readonly ISaveGameRepository saveRepository;
    private readonly IPlayerStateRepository playerRepository;
    private readonly IEnemyStateRepository enemyRepository;
    private readonly IGameplayProgressRepository progressRepository;
    private readonly IGameSessionState sessionState;
    private readonly ISceneLoader sceneLoader;

    public GameSaveInteractor(
        ISaveGameRepository saveRepository,
        IPlayerStateRepository playerRepository,
        IEnemyStateRepository enemyRepository,
        IGameplayProgressRepository progressRepository,
        IGameSessionState sessionState,
        ISceneLoader sceneLoader)
    {
        this.saveRepository = saveRepository;
        this.playerRepository = playerRepository;
        this.enemyRepository = enemyRepository;
        this.progressRepository = progressRepository;
        this.sessionState = sessionState;
        this.sceneLoader = sceneLoader;
    }

    public bool SaveCurrentGame()
    {
        if (saveRepository == null || playerRepository == null)
        {
            return false;
        }

        if (!playerRepository.TryCapture(out PlayerSaveData playerData))
        {
            return false;
        }

        SaveGameData data = new SaveGameData
        {
            SceneName = SceneManager.GetActiveScene().name,
            Player = playerData,
            Enemies = enemyRepository != null ? enemyRepository.Capture() : new List<EnemySaveData>(),
            Progress = progressRepository != null ? progressRepository.Capture() : new GameplayProgressSaveData()
        };
        data.PlayerPosition = playerData.Position;
        data.PlayerRotation = playerData.Rotation;

        saveRepository.Save(data);
        return true;
    }

    public bool LoadGame()
    {
        if (saveRepository == null || sessionState == null || sceneLoader == null)
        {
            return false;
        }

        if (!saveRepository.TryLoad(out SaveGameData loadedData))
        {
            return false;
        }

        sessionState.SetPendingLoadedGame(loadedData);
        sceneLoader.LoadScene(loadedData.SceneName);
        return true;
    }

    public bool ApplyPendingLoadedGame()
    {
        if (sessionState == null || playerRepository == null)
        {
            return false;
        }

        SaveGameData pending = sessionState.PeekPendingLoadedGame();
        if (pending == null || pending.Player == null)
        {
            return false;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.Equals(activeSceneName, pending.SceneName, System.StringComparison.Ordinal))
        {
            return false;
        }

        playerRepository.Restore(pending.Player);
        enemyRepository?.Restore(pending.Enemies);
        progressRepository?.Restore(pending.Progress);
        sessionState.ClearPendingLoadedGame();
        return true;
    }
}

