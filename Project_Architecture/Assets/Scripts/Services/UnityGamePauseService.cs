using UnityEngine;

public sealed class UnityGamePauseService : IGamePauseService
{
    public bool IsPaused => Time.timeScale <= 0f;

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }
}
