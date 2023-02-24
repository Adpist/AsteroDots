using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct MovementSystem : ISystem
{
    public static readonly float HALF_ARENA_WIDTH = 50;
    public static readonly float HALF_ARENA_HEIGHT = 50;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MovementData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach(var (transform, movement) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MovementData>>())
        {
            Vector3 acceleration = movement.ValueRO.acceleration;
            Vector3 velocity = movement.ValueRO.velocity;
            Vector3 position = transform.ValueRO.Position;
            velocity += acceleration * deltaTime;
            float sqrMaxSpeed = movement.ValueRO.maxSpeed * movement.ValueRO.maxSpeed;
            if (velocity.sqrMagnitude > sqrMaxSpeed)
            {
                velocity.Normalize();
                velocity *= movement.ValueRO.maxSpeed;
            }
            position += velocity * deltaTime;
            HandleWrap(ref position);
            movement.ValueRW.velocity = velocity;
            transform.ValueRW.Position = position;
            Quaternion rotation = transform.ValueRO.Rotation;
            transform.ValueRW.Rotation = rotation * Quaternion.Euler(0, 0, movement.ValueRO.angularVelocity * deltaTime);
        }
    }

    [BurstCompile]
    private void HandleWrap(ref Vector3 pos)
    {
        if (pos.x < -HALF_ARENA_WIDTH)
        {
            pos.x += HALF_ARENA_WIDTH * 2.0f;
        }
        else if (pos.x > HALF_ARENA_WIDTH)
        {
            pos.x -= HALF_ARENA_WIDTH * 2.0f;
        }

        if (pos.y < -HALF_ARENA_HEIGHT)
        {
            pos.y += HALF_ARENA_HEIGHT * 2.0f;
        }
        else if (pos.y > HALF_ARENA_HEIGHT)
        {
            pos.y -= HALF_ARENA_HEIGHT * 2.0f;
        }
    }
}
