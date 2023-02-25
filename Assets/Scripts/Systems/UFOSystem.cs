using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct UFOSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UFOData>();
        state.RequireForUpdate<PlayerDesignData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PlayerAspect player = SystemAPI.GetAspectRO<PlayerAspect>(SystemAPI.GetSingletonEntity<PlayerDesignData>());
        float3 playerPosition = player.WorldPosition;
        foreach (var (transform, movement, ufoData) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<MovementData>, RefRW<UFOData>>())
        {
            float sqrChaseRadius = ufoData.ValueRO.chaseRadius * ufoData.ValueRO.chaseRadius;
            if (math.distancesq(transform.ValueRO.Position, playerPosition) <= sqrChaseRadius)
            {
                Vector3 dir = playerPosition - transform.ValueRO.Position;
                dir.Normalize();
                movement.ValueRW.acceleration = dir * ufoData.ValueRO.chaseAcceleration;
                movement.ValueRW.maxSpeed = ufoData.ValueRO.chaseMaxSpeed;
            }
            else
            {
                if (SystemAPI.Time.ElapsedTime >= ufoData.ValueRO.nextDirectionChangeTick)
                {
                    Vector3 dir = UnityEngine.Random.onUnitSphere;
                    dir.z = 0;
                    dir.Normalize();
                    movement.ValueRW.acceleration = dir * ufoData.ValueRO.wanderAcceleration;
                    ufoData.ValueRW.nextDirectionChangeTick = UnityEngine.Random.Range(ufoData.ValueRO.minWanderDirectionChangeDelay, ufoData.ValueRO.maxWanderDirectionChangeDelay);
                }
                movement.ValueRW.maxSpeed = ufoData.ValueRO.wanderMaxSpeed;
            }
        }
    }
}