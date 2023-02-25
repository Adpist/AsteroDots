using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct CollisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerDesignData>();
        state.RequireForUpdate<PlayerRuntimeData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerDesignData>();
        PlayerAspect playerRW = SystemAPI.GetAspectRW<PlayerAspect>(playerEntity);

        NativeArray<Entity> bullets = state.EntityManager.CreateEntityQuery(typeof(BulletTag)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> PowerUps = state.EntityManager.CreateEntityQuery(typeof(PowerUpData)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> enemies = state.EntityManager.CreateEntityQuery(typeof(EnemyData)).ToEntityArray(Allocator.Temp);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int i = 0; i < enemies.Length; ++i)
        {
            Entity enemy = enemies[i];
            EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(enemy);
            bool collided = false;

            for (int j = i + 1; j < enemies.Length; ++j)
            {
                Entity enemy2 = enemies[j];
                if (SphereSphereCollision(ref state, enemy, enemy2))
                {
                    CollisionResponse(ref state, ref ecb, enemy, enemy2);
                    collided = true;
                    break;
                }
            }

            if (collided)
            {
                continue;
            }

            foreach (Entity bullet in bullets)
            {
                if (SphereSphereCollision(ref state, bullet, enemy))
                {
                    ecb.DestroyEntity(bullet);

                    switch (enemyData.enemyType)
                    {
                        case EnemyType.Asteroid:
                            ecb.AddComponent(enemy, typeof(HitAsteroidTag));
                            ecb.RemoveComponent(enemy, typeof(EnemyData));
                            break;

                        case EnemyType.UFO:
                            ecb.DestroyEntity(enemy);
                            break;
                    }
                    Game.instance.AddToScore(enemyData.score);
                    collided = true;
                    break;
                }
            }

            if (collided)
            {
                continue;
            }

            if (!playerRW.IsInvulnerable(SystemAPI.Time.ElapsedTime))
            {
                if (SphereSphereCollision(ref state, playerEntity, enemy))
                {
                    playerRW.SetDead(true);
                }
            }
        }

        foreach (Entity powerUp in PowerUps)
        {
            PowerUpData powerUpData = state.EntityManager.GetComponentData<PowerUpData>(powerUp);
            if (!playerRW.IsDead)
            {
                if (SphereSphereCollision(ref state, playerEntity, powerUp))
                {
                    playerRW.PickupPowerUp(powerUpData.type, SystemAPI.Time.ElapsedTime, powerUpData.duration);
                    ecb.DestroyEntity(powerUp);
                }
            }
        }

        bullets.Dispose();
        enemies.Dispose();
        PowerUps.Dispose();

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public bool SphereSphereCollision(ref SystemState state, Entity entityA, Entity entityB)
    {
        float3 posA = state.EntityManager.GetComponentData<LocalTransform>(entityA).Position;
        float radiusA = state.EntityManager.GetComponentData<SphereColliderData>(entityA).radius;
        float3 posB = state.EntityManager.GetComponentData<LocalTransform>(entityB).Position;
        float radiusB = state.EntityManager.GetComponentData<SphereColliderData>(entityB).radius;
        float radiusSum = radiusA + radiusB;
        return math.distancesq(posA, posB) <= radiusSum * radiusSum;
    }

    [BurstCompile]
    public void CollisionResponse(ref SystemState state, ref EntityCommandBuffer ecb, Entity entityA, Entity entityB)
    {
        float3 posA = state.EntityManager.GetComponentData<LocalTransform>(entityA).Position;
        float3 posB = state.EntityManager.GetComponentData<LocalTransform>(entityB).Position;
        MovementData movementA = state.EntityManager.GetComponentData<MovementData>(entityA);
        MovementData movementB = state.EntityManager.GetComponentData<MovementData>(entityB);
        float3 AtoB = posB - posA;
        float dist = math.length(AtoB);
        float3 AtoBNormal = AtoB/ dist;

        if (math.dot(movementA.velocity, AtoBNormal) > 0)
        {
            movementA.velocity -= 2.0f * math.dot(movementA.velocity, AtoBNormal) * AtoBNormal;
            ecb.SetComponent(entityA, movementA);
        }
        if (math.dot(movementB.velocity, -AtoBNormal) > 0)
        {
            movementB.velocity -= 2.0f * math.dot(movementB.velocity, AtoBNormal) * AtoBNormal;
            ecb.SetComponent(entityB, movementB);
        }
    }
}
