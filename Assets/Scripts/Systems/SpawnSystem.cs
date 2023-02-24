using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct SpawnSystem : ISystem
{

    public bool needReinit;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManagerData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity spawnManager = SystemAPI.GetSingletonEntity<SpawnManagerData>();
        SpawnManagerData spawnData = state.EntityManager.GetComponentData<SpawnManagerData>(spawnManager);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        if (!spawnData.initialSpawnProcessed)
        {
            for (int i = 0; i < spawnData.initialAsteroidsCount; ++i)
            {
                float speed = UnityEngine.Random.Range(spawnData.minAsteroidSpeed, spawnData.maxAsteroidSpeed);
                SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, GetEnnemySpawnPos(Vector3.zero, spawnData.sqrSafetyRadius), GetAsteroidVelocityDirection(), speed, 8);
            }
            spawnData.nextAsteroidSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minAsteroidSpawnDelay, spawnData.maxAsteroidSpawnDelay);
            spawnData.initialSpawnProcessed = true;
            spawnData.nextUFOSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minUFOSpawnDelay, spawnData.maxUFOSpawnDelay);
            state.EntityManager.SetComponentData(spawnManager, spawnData);
        }
        else
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerStats>();
            LocalTransform playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
            if (SystemAPI.Time.ElapsedTime > spawnData.nextAsteroidSpawnTick)
            {
                float speed = UnityEngine.Random.Range(spawnData.minAsteroidSpeed, spawnData.maxAsteroidSpeed);
                SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, GetEnnemySpawnPos(playerTransform.Position, spawnData.sqrSafetyRadius), GetAsteroidVelocityDirection(), speed, 8);
                spawnData.nextAsteroidSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minAsteroidSpawnDelay, spawnData.maxAsteroidSpawnDelay);
                state.EntityManager.SetComponentData(spawnManager, spawnData);
            }

            if(SystemAPI.Time.ElapsedTime > spawnData.nextUFOSpawnTick)
            {
                SpawnUFO(ref ecb, spawnData.ufoPrefab, GetEnnemySpawnPos(playerTransform.Position, spawnData.sqrSafetyRadius));
                spawnData.nextUFOSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minUFOSpawnDelay, spawnData.maxUFOSpawnDelay);
                state.EntityManager.SetComponentData(spawnManager, spawnData);
            }

            NativeArray<Entity> asteroids = state.EntityManager.CreateEntityQuery(typeof(AsteroidStats)).ToEntityArray(Allocator.Temp);
            foreach(Entity asteroid in asteroids)
            {
                AsteroidStats asteroidStats = state.EntityManager.GetComponentData<AsteroidStats>(asteroid);
                if (asteroidStats.destroyed)
                {
                    if (asteroidStats.size > 2)
                    {
                        Movement asteroidMovement = state.EntityManager.GetComponentData<Movement>(asteroid);
                        Vector3 asteroidPos = state.EntityManager.GetComponentData<LocalTransform>(asteroid).Position;
                        Vector3 asteroidVelocity = asteroidMovement.velocity;
                        float speed = asteroidVelocity.magnitude;
                        asteroidVelocity /= speed;
                        Vector3 perpDir = Vector3.Cross(asteroidVelocity, Vector3.forward);
                        int childSize = asteroidStats.size / 2;
                        Vector3 firstChildVelocityDir = asteroidVelocity + perpDir;
                        firstChildVelocityDir.Normalize();
                        Vector3 secondChildVelocityDir = asteroidVelocity - perpDir;
                        secondChildVelocityDir.Normalize();
                        SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, asteroidPos + firstChildVelocityDir * childSize, firstChildVelocityDir, speed * UnityEngine.Random.Range(0.5f, 1.5f), childSize);
                        SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, asteroidPos + secondChildVelocityDir * childSize, secondChildVelocityDir, speed * UnityEngine.Random.Range(0.5f, 1.5f), childSize);
                    }
                    state.EntityManager.DestroyEntity(asteroid);
                }
            }

            NativeArray<Entity> ufos = state.EntityManager.CreateEntityQuery(typeof(UFOData)).ToEntityArray(Allocator.Temp);
            foreach (Entity ufo in ufos)
            {
                UFOData ufoData = state.EntityManager.GetComponentData<UFOData>(ufo);
                if (ufoData.destroyed)
                {
                    state.EntityManager.DestroyEntity(ufo);
                }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    private void SpawnAsteroid(ref EntityCommandBuffer ecb, Entity prefab, Vector3 pos, Vector3 velocityDir, float speed, int asteroidSize)
    {
        Entity newAsteroid = ecb.Instantiate(prefab);
        ecb.SetComponent(newAsteroid, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = asteroidSize });
        ecb.SetComponent(newAsteroid, new Movement { acceleration = Vector3.zero, velocity = velocityDir * speed, angularVelocity = 0, maxSpeed = speed });
        ecb.SetComponent(newAsteroid, new SphereColliderData { radius = asteroidSize/2 });
        ecb.SetComponent(newAsteroid, new AsteroidStats { destroyed = false, size = asteroidSize });
    }

    private void SpawnUFO(ref EntityCommandBuffer ecb, Entity prefab, Vector3 pos)
    {
        Entity newUFO = ecb.Instantiate(prefab);
        ecb.SetComponent(newUFO, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = 1 });
    }

    [BurstCompile]
    public Vector3 GetEnnemySpawnPos(Vector3 safetyCenter, float sqrSafetyRadius)
    {
        Vector3 spawnPos = Vector3.zero;
        do
        {
            spawnPos.x = UnityEngine.Random.Range(-MovementSystem.HALF_ARENA_WIDTH, MovementSystem.HALF_ARENA_WIDTH);
            spawnPos.y = UnityEngine.Random.Range(-MovementSystem.HALF_ARENA_HEIGHT, MovementSystem.HALF_ARENA_HEIGHT);
        } while (math.distancesq(spawnPos, safetyCenter) <= sqrSafetyRadius);
        return spawnPos;
    }

    [BurstCompile]
    public Vector3 GetAsteroidVelocityDirection()
    {
        Vector3 dir = UnityEngine.Random.onUnitSphere;
        dir.z = 0;
        dir.Normalize();
        return dir;
    }
}
