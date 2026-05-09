using UnityEngine.Events;
using UnityEngine.UI;

public static class UiBindingUtility
{
    public static void Add(Button button, UnityAction callback)
    {
        if (button != null)
        {
            button.onClick.AddListener(callback);
        }
    }

    public static void Remove(Button button, UnityAction callback)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(callback);
        }
    }

    public static void Add(Slider slider, UnityAction<float> callback)
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(callback);
        }
    }

    public static void Remove(Slider slider, UnityAction<float> callback)
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(callback);
        }
    }
}
