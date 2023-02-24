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
        state.RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity player = SystemAPI.GetSingletonEntity<PlayerData>();
        PlayerData playerData = state.EntityManager.GetComponentData<PlayerData>(player);
        NativeArray<Entity> bullets = state.EntityManager.CreateEntityQuery(typeof(BulletData)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> PowerUps = state.EntityManager.CreateEntityQuery(typeof(PowerUpData)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> enemies = state.EntityManager.CreateEntityQuery(typeof(EnemyData)).ToEntityArray(Allocator.Temp);

        foreach (Entity enemy in enemies)
        {
            EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(enemy);
            if (!enemyData.destroyed)
            {
                foreach (Entity bullet in bullets)
                {
                    BulletData bulletData = state.EntityManager.GetComponentData<BulletData>(bullet);
                    if (bulletData.lifeTime > 0)
                    {
                        if (SphereSphereCollision(bullet, enemy, ref state))
                        {
                            bulletData.lifeTime = 0;
                            state.EntityManager.SetComponentData(bullet, bulletData);
                            enemyData.destroyed = true;
                            state.EntityManager.SetComponentData(enemy, enemyData);
                            Game.instance.AddToScore(enemyData.score);
                        }
                    }
                }
            }

            if (!playerData.dead && SystemAPI.Time.ElapsedTime >= playerData.invulnerabilityExpireTick)
            {
                if (SphereSphereCollision(player, enemy, ref state))
                {
                    playerData.dead = true;
                    state.EntityManager.SetComponentData(player, playerData);
                }
            }
        }

        foreach (Entity powerUp in PowerUps)
        {
            PowerUpData powerUpData = state.EntityManager.GetComponentData<PowerUpData>(powerUp);
            if (!powerUpData.picked)
            {
                if (!playerData.dead)
                {
                    if (SphereSphereCollision(player, powerUp, ref state))
                    {
                        powerUpData.picked = true;
                        switch(powerUpData.type)
                        {
                            case PowerUpType.Shield:
                                playerData.invulnerabilityExpireTick = SystemAPI.Time.ElapsedTime + (double)powerUpData.duration;
                                break;

                            case PowerUpType.MultiShoot:
                                playerData.multiShootExpireTick = SystemAPI.Time.ElapsedTime + (double)powerUpData.duration;
                                break;
                        }
                        state.EntityManager.SetComponentData(powerUp, powerUpData);
                        state.EntityManager.SetComponentData(player, playerData);
                    }
                }
            }
        }

        bullets.Dispose();
        enemies.Dispose();
        PowerUps.Dispose();
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
