using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool accelerating = Input.GetKey(KeyCode.UpArrow);
        bool shooting = Input.GetKey(KeyCode.Space);

        foreach (var (playerStats, transform, movement) in SystemAPI.Query<RefRO<PlayerStats>, RefRW<LocalTransform>, RefRW<Movement>>())
        {
            float angularSpeed = 0;
            Vector3 acceleration = Vector3.zero;
            if (left)
            {
                angularSpeed += playerStats.ValueRO.rotationSpeed;
            }

            if (right)
            {
                angularSpeed -= playerStats.ValueRO.rotationSpeed;
            }

            if (accelerating)
            {
                acceleration = transform.ValueRO.Up();
                acceleration *= playerStats.ValueRO.accelerationSpeed;
            }

            movement.ValueRW.angularVelocity = angularSpeed;
            movement.ValueRW.acceleration = acceleration;
        }
    }
}
