using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
partial struct ExpireEntitiesJob : IJobEntity
{
    public double currentTick;
    public EntityCommandBuffer.ParallelWriter ECB;

    [BurstCompile]
    public void Execute(LifeTimeData lifeTimeData, Entity entity, [EntityIndexInQuery]int sortKey)
    {
        if (currentTick >= lifeTimeData.expirationTick)
        {
            ECB.DestroyEntity(sortKey, entity);
        }
    }
}

[BurstCompile]
public partial struct LifeTimeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LifeTimeData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        new ExpireEntitiesJob
        {
            currentTick = SystemAPI.Time.ElapsedTime,
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel();
    }
}
