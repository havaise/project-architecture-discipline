public sealed class EnemyConfigInitializer
{
    public void ApplyDefaults(
        ref EnemyVisionConfig visionConfig,
        ref EnemyMovementConfig movementConfig,
        ref EnemyAttackConfig attackConfig,
        ref EnemyProjectileConfig projectileConfig,
        ref EnemyBehaviourConfig behaviourConfig)
    {
        if (visionConfig.ViewDistance <= 0f)
        {
            visionConfig = EnemyVisionConfig.CreateDefault();
        }

        if (movementConfig.MoveSpeed <= 0f)
        {
            movementConfig = EnemyMovementConfig.CreateDefault();
        }
        else
        {
            ApplyMovementDefaults(ref movementConfig);
        }

        if (attackConfig.AttackCooldown <= 0f)
        {
            attackConfig = EnemyAttackConfig.CreateDefault();
        }

        if (projectileConfig.ProjectileLifetime <= 0f)
        {
            projectileConfig = EnemyProjectileConfig.CreateDefault();
        }

        if (behaviourConfig.FleeDistance <= 0f)
        {
            behaviourConfig = EnemyBehaviourConfig.CreateDefault();
        }
    }

    private void ApplyMovementDefaults(ref EnemyMovementConfig movementConfig)
    {
        if (movementConfig.NavAcceleration <= 0f)
        {
            movementConfig.NavAcceleration = 16f;
        }

        if (movementConfig.NavAngularSpeed <= 0f)
        {
            movementConfig.NavAngularSpeed = 540f;
        }

        if (movementConfig.NavStoppingDistance < 0f)
        {
            movementConfig.NavStoppingDistance = 0.8f;
        }

        if (movementConfig.NavPathRepathInterval <= 0f)
        {
            movementConfig.NavPathRepathInterval = 0.2f;
        }
    }
}
