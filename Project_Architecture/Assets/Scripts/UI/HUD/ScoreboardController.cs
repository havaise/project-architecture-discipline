using System;

public sealed class ScoreboardController : IDisposable
{
    private readonly ScoreboardModel model;
    private readonly ScoreboardView view;
    private bool initialized;

    public ScoreboardController(ScoreboardModel model, ScoreboardView view)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
        this.view = view;
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        model.Changed += OnScoreboardChanged;
        view?.Render(model.Score, model.Kills);
        initialized = true;
    }

    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        model.Changed -= OnScoreboardChanged;
        initialized = false;
    }

    private void OnScoreboardChanged(int score, int kills)
    {
        view?.Render(score, kills);
    }
}
