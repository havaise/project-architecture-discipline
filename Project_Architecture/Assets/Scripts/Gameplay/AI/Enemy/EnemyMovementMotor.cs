using System;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyMovementMotor
{
    private const float DestinationUpdateThreshold = 0.25f;
    private const float MinRepathInterval = 0.05f;

    private readonly Transform transform;
    private readonly NavMeshAgent navMeshAgent;
    private readonly EnemyAiModel aiModel;
    private Vector3 lastDestination;
    private bool hasDestination;
    private float nextAllowedRepathTime;
    private bool navMeshWarningShown;

    public EnemyMovementMotor(Transform transform, NavMeshAgent navMeshAgent, EnemyAiModel aiModel)
    {
        this.transform = transform != null
            ? transform
            : throw new ArgumentNullException(nameof(transform));
        this.navMeshAgent = navMeshAgent;
        this.aiModel = aiModel != null
            ? aiModel
            : throw new ArgumentNullException(nameof(aiModel));

        if (this.navMeshAgent == null)
        {
            Debug.LogWarning("[EnemyMovementMotor] NavMeshAgent is missing. Enemy movement is disabled.", this.transform);
            return;
        }

        this.navMeshAgent.updatePosition = true;
        this.navMeshAgent.updateRotation = true;
        this.navMeshAgent.autoBraking = true;
    }

    public void ConfigurePhysics(Rigidbody body, bool configureRigidbodyForNavMesh)
    {
        if (!configureRigidbodyForNavMesh || body == null || navMeshAgent == null)
        {
            return;
        }

        body.useGravity = false;
        body.isKinematic = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void RotateIdle(bool enabled, float idleTurnSpeed, float deltaTime)
    {
        if (!enabled || !UseNavMeshAgent())
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        transform.Rotate(Vector3.up, idleTurnSpeed * deltaTime, Space.World);
    }

    public void Chase(
        Vector3 destination,
        float moveSpeed,
        float acceleration,
        float angularSpeed,
        float stoppingDistance,
        float repathInterval,
        float currentTime)
    {
        if (!UseNavMeshAgent())
        {
            aiModel.SetMoveState(false, 0f);
            return;
        }

        ChaseWithNavMesh(
            destination,
            moveSpeed,
            acceleration,
            angularSpeed,
            stoppingDistance,
            repathInterval,
            currentTime);
    }

    public void Stop()
    {
        aiModel.SetMoveState(false, 0f);

        if (!UseNavMeshAgent())
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        hasDestination = false;
    }

    private void ChaseWithNavMesh(
        Vector3 destination,
        float moveSpeed,
        float acceleration,
        float angularSpeed,
        float stoppingDistance,
        float repathInterval,
        float currentTime)
    {
        navMeshAgent.acceleration = Mathf.Max(0.1f, acceleration);
        navMeshAgent.angularSpeed = Mathf.Max(1f, angularSpeed);
        navMeshAgent.speed = Mathf.Max(0.01f, moveSpeed);
        navMeshAgent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
        navMeshAgent.isStopped = false;

        bool destinationChanged =
            !hasDestination
            || (destination - lastDestination).sqrMagnitude >= DestinationUpdateThreshold * DestinationUpdateThreshold;
        float repathDelay = Mathf.Max(MinRepathInterval, repathInterval);
        bool canRepathNow = currentTime >= nextAllowedRepathTime;
        if (destinationChanged && canRepathNow)
        {
            if (navMeshAgent.SetDestination(destination))
            {
                lastDestination = destination;
                hasDestination = true;
                nextAllowedRepathTime = currentTime + repathDelay;
            }
        }

        if (!navMeshAgent.pathPending && navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            aiModel.SetMoveState(false, 0f);
            return;
        }

        float effectiveSpeed = navMeshAgent.velocity.magnitude;
        float speed01 = navMeshAgent.speed > 0.01f
            ? Mathf.Clamp01(effectiveSpeed / navMeshAgent.speed)
            : 0f;

        bool hasArrived = !navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.05f;

        if (hasArrived)
        {
            speed01 = 0f;
        }

        aiModel.SetMoveState(speed01 > 0.01f, speed01);
    }

    private bool UseNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            return false;
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            if (!navMeshWarningShown)
            {
                Debug.LogWarning("[EnemyMovementMotor] Agent is not on NavMesh. Movement is paused.", transform);
                navMeshWarningShown = true;
            }

            return false;
        }

        navMeshWarningShown = false;
        return true;
    }
}
