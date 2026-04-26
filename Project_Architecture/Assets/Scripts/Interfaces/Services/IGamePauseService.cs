public interface IGamePauseService
{
    bool IsPaused { get; }
    void Pause();
    void Resume();
}
