public sealed class BossConfigInitializer
{
    public void ApplyDefaults(
        ref EnemyVisionConfig visionConfig,
        ref EnemyMovementConfig movementConfig,
        ref EnemyAttackConfig attackConfig,
        ref EnemyProjectileConfig projectileConfig,
        ref BossCombatConfig bossCombatConfig,
        ref float deaggroDistance,
        ref float deaggroDelay)
    {
        EnemyConfigInitializer enemyConfigInitializer = new EnemyConfigInitializer();
        EnemyBehaviourConfig unusedBehaviourConfig = EnemyBehaviourConfig.CreateDefault();
        enemyConfigInitializer.ApplyDefaults(
            ref visionConfig,
            ref movementConfig,
            ref attackConfig,
            ref projectileConfig,
            ref unusedBehaviourConfig);

        if (bossCombatConfig.StrongAttackCooldown <= 0f)
        {
            bossCombatConfig = BossCombatConfig.CreateDefault();
        }

        deaggroDistance = UnityEngine.Mathf.Max(1f, deaggroDistance);
        deaggroDelay = UnityEngine.Mathf.Max(0.1f, deaggroDelay);
    }
}
