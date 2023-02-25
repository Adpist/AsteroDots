using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;


[BurstCompile]
partial struct UpdateMovementJob : IJobEntity
{
    public float deltaTime;

    [BurstCompile]
    public void Execute(MovableAspect movable)
    {
        movable.UpdateMovement(deltaTime);
    }
}

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct MovementSystem : ISystem
{
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
        new UpdateMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
    
}
