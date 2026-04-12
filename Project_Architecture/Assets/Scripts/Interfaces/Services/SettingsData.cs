using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public float SfxVolume = 1f;

    public SettingsData Clamp()
    {
        SfxVolume = Mathf.Clamp01(SfxVolume);
        return this;
    }
}
