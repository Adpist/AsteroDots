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

        foreach (Entity enemy in enemies)
        {
            EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(enemy);
            foreach (Entity bullet in bullets)
            {
                if (SphereSphereCollision(bullet, enemy, ref state))
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
                }
            }

            if (!playerRW.IsInvulnerable(SystemAPI.Time.ElapsedTime))
            {
                if (SphereSphereCollision(playerEntity, enemy, ref state))
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
                if (SphereSphereCollision(playerEntity, powerUp, ref state))
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
    public bool SphereSphereCollision(Entity entityA, Entity entityB, ref SystemState state)
    {
        Vector3 posA = state.EntityManager.GetComponentData<LocalTransform>(entityA).Position;
        float radiusA = state.EntityManager.GetComponentData<SphereColliderData>(entityA).radius;
        Vector3 posB = state.EntityManager.GetComponentData<LocalTransform>(entityB).Position;
        float radiusB = state.EntityManager.GetComponentData<SphereColliderData>(entityB).radius;
        float radiusSum = radiusA + radiusB;
        return math.distancesq(posA, posB) <= radiusSum * radiusSum;
    }
}
