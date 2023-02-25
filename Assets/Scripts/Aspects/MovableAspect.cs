using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct MovableAspect : IAspect
{
    public readonly Entity entity;

    private readonly TransformAspect transform;
    private readonly RefRW<MovementData> movementData;

    public void UpdateMovement(float deltaTime)
    {
        UpdateVelocity(deltaTime);
        UpdatePosition(deltaTime);
        UpdateRotation(deltaTime);
    }

    public void UpdateVelocity(float deltaTime)
    {
        float3 velocity = movementData.ValueRO.velocity;
        float maxSpeed = movementData.ValueRO.maxSpeed;
        velocity += movementData.ValueRO.acceleration * deltaTime;
        float sqrMaxSpeed = maxSpeed * maxSpeed;
        if (math.lengthsq(velocity) > sqrMaxSpeed)
        {
            velocity = math.normalize(velocity);
            velocity *= maxSpeed;
        }
        movementData.ValueRW.velocity = velocity;
    }

    public void UpdatePosition(float deltaTime)
    {
        float3 position = transform.LocalPosition;
        position += movementData.ValueRO.velocity * deltaTime;
        HandleWrap(ref position);
        transform.LocalPosition = position;
    }

    public void UpdateRotation(float deltaTime)
    {
        transform.LocalRotation = transform.LocalRotation * Quaternion.Euler(0, 0, movementData.ValueRO.angularVelocity * deltaTime);
    }

    private void HandleWrap(ref float3 pos)
    {
        float arenaHalfWidth = Game.instance.arenaWidth * 0.5f;
        float arenaHalfHeight = Game.instance.arenaHeight * 0.5f;

        if (pos.x < -arenaHalfWidth)
        {
            pos.x += Game.instance.arenaWidth;
        }
        else if (pos.x > arenaHalfWidth)
        {
            pos.x -= Game.instance.arenaWidth;
        }

        if (pos.y < -arenaHalfHeight)
        {
            pos.y += Game.instance.arenaHeight;
        }
        else if (pos.y > arenaHalfHeight)
        {
            pos.y -= Game.instance.arenaHeight;
        }
    }
}