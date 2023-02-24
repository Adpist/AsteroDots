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
        state.RequireForUpdate<PlayerStats>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerStats>();
        LocalTransform playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);

        foreach (var (transform, movement, ufoData) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Movement>, RefRW<UFOData>>())
        {
            float sqrChaseRadius = ufoData.ValueRO.chaseRadius * ufoData.ValueRO.chaseRadius;
            if (math.distancesq(transform.ValueRO.Position, playerTransform.Position) <= sqrChaseRadius)
            {
                Vector3 dir = playerTransform.Position - transform.ValueRO.Position;
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