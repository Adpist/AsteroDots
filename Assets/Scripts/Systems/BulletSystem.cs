using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct BulletSystem : ISystem
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
        EntityQuery query = state.EntityManager.CreateEntityQuery(typeof(BulletStats));
        foreach (Entity bulletEntity in query.ToEntityArray(Allocator.Temp))
        {
            BulletStats bullet = state.EntityManager.GetComponentData<BulletStats>(bulletEntity);
            bullet.lifeTime -= SystemAPI.Time.DeltaTime;
            state.EntityManager.SetComponentData(bulletEntity, bullet);
            if (bullet.lifeTime <= 0)
            {
                state.EntityManager.DestroyEntity(bulletEntity);
            }
        }
    }
}
