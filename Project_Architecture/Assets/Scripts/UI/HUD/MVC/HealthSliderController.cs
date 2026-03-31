using UnityEngine;

public sealed class HealthSliderController
{
    private readonly HealthSliderModel model;
    private readonly HealthSliderView view;
    private bool initialized;

    public HealthSliderController(HealthSliderModel model, HealthSliderView view)
    {
        this.model = model;
        this.view = view;
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        model.HealthChanged += OnHealthChanged;
        model.Subscribe();
        view.Render(model.Current, model.Max);
        initialized = true;
    }

    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        model.Unsubscribe();
        model.HealthChanged -= OnHealthChanged;
        initialized = false;
    }

    private void OnHealthChanged(int current, int max)
    {
        view.Render(current, max);
    }
}
