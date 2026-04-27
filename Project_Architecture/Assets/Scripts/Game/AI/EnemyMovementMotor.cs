using System;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyMovementMotor
{
    private const float DestinationUpdateThreshold = 0.35f;
    private const float RotationEpsilon = 0.0004f;

    private readonly Transform transform;
    private readonly NavMeshAgent navMeshAgent;
    private readonly EnemyAiModel aiModel;
    private Vector3 lastDestination;
    private bool hasDestination;

    public EnemyMovementMotor(Transform transform, NavMeshAgent navMeshAgent, EnemyAiModel aiModel)
    {
        this.transform = transform != null
            ? transform
            : throw new ArgumentNullException(nameof(transform));
        this.navMeshAgent = navMeshAgent;
        this.aiModel = aiModel != null
            ? aiModel
            : throw new ArgumentNullException(nameof(aiModel));

        if (this.navMeshAgent != null)
        {
            this.navMeshAgent.updateRotation = false;
        }
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
        if (!enabled)
        {
            return;
        }

        transform.Rotate(Vector3.up, idleTurnSpeed * deltaTime, Space.World);
    }

    public void Chase(Vector3 destination, float moveSpeed, float rotationSpeed, float deltaTime)
    {
        if (UseNavMeshAgent())
        {
            ChaseWithNavMesh(destination, moveSpeed, rotationSpeed, deltaTime);
            return;
        }

        ChaseManually(destination, moveSpeed, rotationSpeed, deltaTime);
    }

    public void Stop()
    {
        aiModel.SetMoveState(false, 0f);

        if (!UseNavMeshAgent())
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        hasDestination = false;
    }

    private void ChaseWithNavMesh(Vector3 destination, float moveSpeed, float rotationSpeed, float deltaTime)
    {
        navMeshAgent.speed = Mathf.Max(0.01f, moveSpeed);
        navMeshAgent.isStopped = false;

        if (!hasDestination || (destination - lastDestination).sqrMagnitude >= DestinationUpdateThreshold * DestinationUpdateThreshold)
        {
            navMeshAgent.SetDestination(destination);
            lastDestination = destination;
            hasDestination = true;
        }

        Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
        desiredVelocity.y = 0f;
        if (desiredVelocity.sqrMagnitude > RotationEpsilon)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }

        float effectiveSpeed = Mathf.Max(navMeshAgent.velocity.magnitude, navMeshAgent.desiredVelocity.magnitude);
        float speed01 = navMeshAgent.speed > 0.01f
            ? Mathf.Clamp01(effectiveSpeed / navMeshAgent.speed)
            : 0f;

        if (speed01 <= 0.01f && Vector3.Distance(transform.position, destination) > 0.2f)
        {
            bool shouldMove =
                navMeshAgent.pathPending
                || (navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 0.05f)
                || Vector3.Distance(transform.position, destination) > 0.2f;

            if (shouldMove)
            {
                speed01 = 1f;
            }
        }

        aiModel.SetMoveState(speed01 > 0.01f, speed01);
    }

    private void ChaseManually(Vector3 destination, float moveSpeed, float rotationSpeed, float deltaTime)
    {
        Vector3 toTarget = destination - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            aiModel.SetMoveState(false, 0f);
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * moveSpeed * deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);

        aiModel.SetMoveState(true, 1f);
    }

    private bool UseNavMeshAgent()
    {
        return navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh;
    }
}
