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
        state.RequireForUpdate<PlayerStats>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity player = SystemAPI.GetSingletonEntity<PlayerStats>();
        PlayerStats playerData = state.EntityManager.GetComponentData<PlayerStats>(player);
        NativeArray<Entity> asteroids = state.EntityManager.CreateEntityQuery(typeof(AsteroidStats)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> bullets = state.EntityManager.CreateEntityQuery(typeof(BulletStats)).ToEntityArray(Allocator.Temp);
        NativeArray<Entity> UFOs = state.EntityManager.CreateEntityQuery(typeof(UFOData)).ToEntityArray(Allocator.Temp);

        foreach (Entity asteroid in asteroids)
        {
            AsteroidStats asteroidData = state.EntityManager.GetComponentData<AsteroidStats>(asteroid);
            if (!asteroidData.destroyed)
            {
                foreach (Entity bullet in bullets)
                {
                    BulletStats bulletData = state.EntityManager.GetComponentData<BulletStats>(bullet);
                    if (bulletData.lifeTime > 0)
                    {
                        if (SphereSphereCollision(bullet, asteroid, ref state))
                        {
                            bulletData.lifeTime = 0;
                            state.EntityManager.SetComponentData(bullet, bulletData);
                            asteroidData.destroyed = true;
                            state.EntityManager.SetComponentData(asteroid, asteroidData);
                            Game.instance.AddToScore(10 * asteroidData.size);
                        }
                    }
                }
            }

            if (!playerData.dead)
            {
                if (SphereSphereCollision(player, asteroid, ref state))
                {
                    playerData.dead = true;
                    state.EntityManager.SetComponentData(player, playerData);
                }
            }
        }

        foreach (Entity ufo in UFOs)
        {
            UFOData ufoData = state.EntityManager.GetComponentData<UFOData>(ufo);
            if (!ufoData.destroyed)
            {
                foreach (Entity bullet in bullets)
                {
                    BulletStats bulletData = state.EntityManager.GetComponentData<BulletStats>(bullet);
                    if (bulletData.lifeTime > 0)
                    {
                        if (SphereSphereCollision(bullet, ufo, ref state))
                        {
                            bulletData.lifeTime = 0;
                            state.EntityManager.SetComponentData(bullet, bulletData);
                            ufoData.destroyed = true;
                            state.EntityManager.SetComponentData(ufo, ufoData);
                            Game.instance.AddToScore(200);
                        }
                    }
                }
            }

            if (!playerData.dead)
            {
                if (SphereSphereCollision(player, ufo, ref state))
                {
                    playerData.dead = true;
                    state.EntityManager.SetComponentData(player, playerData);
                }
            }
        }
    }

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
