using System;
using UnityEngine;

public sealed class ManaModel
{
    private int maxMana;
    private int currentMana;

    public int Current => currentMana;
    public int Max => maxMana;

    public event Action<int, int> ManaChanged;

    public void Initialize(int initialMaxMana, int initialCurrentMana, bool resetToMax)
    {
        maxMana = Mathf.Max(1, initialMaxMana);
        currentMana = resetToMax ? maxMana : Mathf.Clamp(initialCurrentMana, 0, maxMana);
        ManaChanged?.Invoke(currentMana, maxMana);
    }

    public void SetCurrent(int value)
    {
        int newMana = Mathf.Clamp(value, 0, maxMana);
        if (newMana == currentMana)
        {
            return;
        }

        currentMana = newMana;
        ManaChanged?.Invoke(currentMana, maxMana);
    }
}
