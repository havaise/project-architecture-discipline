using UnityEngine;

public interface IPlayerProjectileFactory
{
    MagicProjectile Create(MagicProjectile prefab, Vector3 position, Quaternion rotation);
}

public sealed class UnityPlayerProjectileFactory : IPlayerProjectileFactory
{
    public MagicProjectile Create(MagicProjectile prefab, Vector3 position, Quaternion rotation)
    {
        return prefab != null ? Object.Instantiate(prefab, position, rotation) : null;
    }
}
