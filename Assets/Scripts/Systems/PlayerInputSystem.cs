using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
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
        bool shooting = Input.GetKeyDown(KeyCode.Space);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
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

            if (shooting)
            {
                Entity newBullet = ecb.Instantiate(playerStats.ValueRO.bulletPrefab);
                Vector3 spawnPos = transform.ValueRO.Position + transform.ValueRO.Up();
                Vector3 bulletVelocity = movement.ValueRO.velocity + transform.ValueRO.Up() * playerStats.ValueRO.bulletSpeed;
                ecb.SetComponent(newBullet, new LocalTransform { Position = spawnPos, Rotation = Quaternion.identity, Scale = 1 });
                ecb.SetComponent(newBullet, new Movement { acceleration = Vector3.zero, velocity = bulletVelocity, angularVelocity = 0, maxSpeed = movement.ValueRO.maxSpeed + playerStats.ValueRO.bulletSpeed });
                ecb.SetComponent(newBullet, new BulletStats { lifeTime = playerStats.ValueRO.bulletLifeTime });
            }
        }

        ecb.Playback(state.EntityManager);
    }
}
