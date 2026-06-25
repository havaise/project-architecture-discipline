using System;
using UnityEngine;

public class ManaComponent : MonoBehaviour
{
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int currentMana = 100;
    [SerializeField] private bool resetToMaxOnAwake = true;

    private readonly ManaModel model = new ManaModel();

    public int Current => model.Current;
    public int Max => model.Max;

    public event Action<int, int> ManaChanged;

    private void Awake()
    {
        model.ManaChanged += OnManaChanged;
        model.Initialize(maxMana, currentMana, resetToMaxOnAwake);
    }

    private void OnDestroy()
    {
        model.ManaChanged -= OnManaChanged;
    }

    public void SetCurrent(int value)
    {
        model.SetCurrent(value);
    }

    private void OnManaChanged(int current, int max)
    {
        currentMana = current;
        maxMana = max;
        ManaChanged?.Invoke(current, max);
    }
}
