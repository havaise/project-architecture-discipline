using System;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyAgentFactory
{
    public EnemyAgent Create(EnemyAgentFactoryContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        IEnemyTargetProvider targetProvider = new UnityEnemyTargetProvider(context.PlayerTag);
        IEnemyProjectileFactory projectileFactory = new UnityEnemyProjectileFactory(
            context.ProjectileConfig.ProjectilePrefab,
            context.Log);
        EnemyAiModel aiModel = new EnemyAiModel(
            context.VisionConfig.VisibilityMemoryDuration,
            context.AttackConfig.AttackCooldown);
        EnemyVisionSensor visionSensor = new EnemyVisionSensor(context.Owner);
        EnemyMovementMotor movementMotor = new EnemyMovementMotor(
            context.Owner,
            context.NavMeshAgent,
            aiModel);
        EnemyAttackSystem attackSystem = new EnemyAttackSystem(
            context.Owner,
            aiModel,
            _ => projectileFactory.CreateProjectile());

        movementMotor.ConfigurePhysics(
            context.Body,
            context.MovementConfig.ConfigureRigidbodyForNavMesh);

        return new EnemyAgent(
            context.Owner,
            context.InitialTarget,
            aiModel,
            visionSensor,
            movementMotor,
            attackSystem,
            targetProvider,
            context.VisionConfig,
            context.MovementConfig,
            context.AttackConfig,
            context.ProjectileConfig,
            context.BehaviourConfig,
            context.AutoFindPlayer,
            context.GetDamage,
            context.GetHealthRatio,
            context.HandleAttackResult,
            context.EnableCombatDebugLogs);
    }
}

public sealed class EnemyAgentFactoryContext
{
    public Transform Owner { get; set; }
    public Transform InitialTarget { get; set; }
    public NavMeshAgent NavMeshAgent { get; set; }
    public Rigidbody Body { get; set; }
    public string PlayerTag { get; set; }
    public bool AutoFindPlayer { get; set; }
    public EnemyVisionConfig VisionConfig { get; set; }
    public EnemyMovementConfig MovementConfig { get; set; }
    public EnemyAttackConfig AttackConfig { get; set; }
    public EnemyProjectileConfig ProjectileConfig { get; set; }
    public EnemyBehaviourConfig BehaviourConfig { get; set; }
    public Func<int> GetDamage { get; set; }
    public Func<float> GetHealthRatio { get; set; }
    public Action<EnemyAttackResult, float> HandleAttackResult { get; set; }
    public Action<string> Log { get; set; }
    public bool EnableCombatDebugLogs { get; set; }
}
