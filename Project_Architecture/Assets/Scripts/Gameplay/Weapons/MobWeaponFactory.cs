using UnityEngine;

public sealed class MobWeaponFactory
{
    public MobWeaponRuntime Create(
        MobWeaponConfig defaultWeapon,
        MobWeaponConfig[] availableWeapons,
        bool randomize)
    {
        MobWeaponConfig selectedWeapon = SelectWeapon(defaultWeapon, availableWeapons, randomize);
        return selectedWeapon != null ? new MobWeaponRuntime(selectedWeapon) : null;
    }

    private MobWeaponConfig SelectWeapon(
        MobWeaponConfig defaultWeapon,
        MobWeaponConfig[] availableWeapons,
        bool randomize)
    {
        if (availableWeapons != null && availableWeapons.Length > 0)
        {
            if (!randomize)
            {
                return availableWeapons[0];
            }

            int index = Random.Range(0, availableWeapons.Length);
            return availableWeapons[index];
        }

        return defaultWeapon;
    }
}
