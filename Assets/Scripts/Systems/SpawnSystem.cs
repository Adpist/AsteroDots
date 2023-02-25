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
    SpawnAspect spawnRO;
    SpawnAspect spawnRW;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnDesignData>();
        state.RequireForUpdate<SpawnAsteroidDesignData>();
        state.RequireForUpdate<SpawnUFODesignData>();
        state.RequireForUpdate<SpawnPowerUpDesignData>();
        state.RequireForUpdate<SpawnRuntimeData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity spawnEntity = SystemAPI.GetSingletonEntity<SpawnDesignData>();
        spawnRW = SystemAPI.GetAspectRW<SpawnAspect>(spawnEntity);
        spawnRO = SystemAPI.GetAspectRO<SpawnAspect>(spawnEntity);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        if (spawnRO.NeedProcessInitialSpawn())
        {
            ProcessInitialSpawn(ref state, ref ecb);
        }
        else
        {
            PlayerAspect player = SystemAPI.GetAspectRO<PlayerAspect>(SystemAPI.GetSingletonEntity<PlayerDesignData>());
            float3 playerPosition = player.WorldPosition;

            UpdateAsteroidSpawn(ref state, ref ecb, playerPosition);
            UpdateUFOSpawn(ref state, ref ecb, playerPosition);
            UpdatePowerUpSpawn(ref state, ref ecb);

            NativeArray<Entity> asteroids = state.EntityManager.CreateEntityQuery(typeof(AsteroidData)).ToEntityArray(Allocator.Temp);
            foreach(Entity asteroid in asteroids)
            {
                AsteroidData asteroidData = state.EntityManager.GetComponentData<AsteroidData>(asteroid);
                EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(asteroid);
                if (enemyData.destroyed)
                {
                    if (spawnRO.CanSplitAsteroid(asteroidData.size))
                    {
                        float3 asteroidPos = state.EntityManager.GetComponentData<LocalTransform>(asteroid).Position;
                        float3 asteroidVelocity = state.EntityManager.GetComponentData<MovementData>(asteroid).velocity;
                        SplitAsteroid(ref ecb, asteroidPos, asteroidVelocity, asteroidData.size);
                    }
                    state.EntityManager.DestroyEntity(asteroid);
                }
            }

            NativeArray<Entity> ufos = state.EntityManager.CreateEntityQuery(typeof(UFOData)).ToEntityArray(Allocator.Temp);
            foreach (Entity ufo in ufos)
            {
                EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(ufo);
                if (enemyData.destroyed)
                {
                    state.EntityManager.DestroyEntity(ufo);
                }
            }

            NativeArray<Entity> powerUps = state.EntityManager.CreateEntityQuery(typeof(PowerUpData)).ToEntityArray(Allocator.Temp);
            foreach (Entity powerUp in powerUps)
            {
                PowerUpData powerUpData = state.EntityManager.GetComponentData<PowerUpData>(powerUp);
                if (powerUpData.picked)
                {
                    state.EntityManager.DestroyEntity(powerUp);
                }
            }

            asteroids.Dispose();
            ufos.Dispose();
            powerUps.Dispose();
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    private void UpdateAsteroidSpawn(ref SystemState state, ref EntityCommandBuffer ecb, float3 playerPosition)
    {
        if (spawnRO.NeedToSpawnAsteroid(SystemAPI.Time.ElapsedTime))
        {
            SpawnBigAsteroid(ref ecb, playerPosition);
            spawnRW.ComputeNextAsteroidSpawn(SystemAPI.Time.ElapsedTime);
        }
    }

    [BurstCompile]
    private void UpdateUFOSpawn(ref SystemState state, ref EntityCommandBuffer ecb, float3 playerPosition)
    {
        if (spawnRO.NeedToSpawnUFO(SystemAPI.Time.ElapsedTime))
        {
            float3 spawnPos = spawnRO.GetRandomEnemySpawnPos(playerPosition);
            SpawnUFO(ref ecb, spawnPos);
            spawnRW.ComputeNextUFOSpawn(SystemAPI.Time.ElapsedTime);
        }
    }

    [BurstCompile]
    private void UpdatePowerUpSpawn(ref SystemState state, ref EntityCommandBuffer ecb)
    {
        if (spawnRO.NeedToSpawnPowerUp(SystemAPI.Time.ElapsedTime))
        {
            PowerUpType type = spawnRO.GetRandomPowerUpType();
            float3 spawnPos = spawnRO.GetRandomSpawnPos();
            switch (type)
            {
                case PowerUpType.Shield:
                    SpawnPowerUp(ref ecb, spawnRO.ShieldPowerUpPrefab, spawnPos);
                    break;

                case PowerUpType.MultiShoot:
                    SpawnPowerUp(ref ecb, spawnRO.MultiShootPowerUpPrefab, spawnPos);
                    break;
            }
            spawnRW.ComputeNextPowerUpSpawn(SystemAPI.Time.ElapsedTime);
        }
    }

    [BurstCompile]
    private void ProcessInitialSpawn(ref SystemState state, ref EntityCommandBuffer ecb)
    {
        for (int i = 0; i < spawnRO.InitialAsteroidsToSpawn; ++i)
        {
            SpawnBigAsteroid(ref ecb, float3.zero);
        }
        spawnRW.ComputeNextAsteroidSpawn(SystemAPI.Time.ElapsedTime);
        spawnRW.ComputeNextUFOSpawn(SystemAPI.Time.ElapsedTime);
        spawnRW.ComputeNextPowerUpSpawn(SystemAPI.Time.ElapsedTime);
        spawnRW.SetInitialSpawnProcessed(true);
    }

    [BurstCompile]
    private void SpawnBigAsteroid(ref EntityCommandBuffer ecb, float3 playerPosition)
    {
        float speed = spawnRO.GetRandomAsteroidSpeed();
        float3 spawnPos = spawnRO.GetRandomEnemySpawnPos(playerPosition);
        float3 velocityDir = spawnRO.GetRandomDirection();
        SpawnAsteroid(ref ecb, spawnPos, velocityDir, speed, spawnRO.AsteroidSpawnSize);
    }

    [BurstCompile]
    private void SplitAsteroid(ref EntityCommandBuffer ecb, float3 srcPosition, float3 srcVelocity, int srcSize)
    {
        float speed = math.length(srcVelocity);
        srcVelocity /= speed;
        float3 perpDir = math.cross(srcVelocity, new float3(0,0,1));
        int childSize = srcSize / 2;

        SpawnChildAsteroid(ref ecb, srcPosition, srcVelocity, perpDir, speed, childSize);
        SpawnChildAsteroid(ref ecb, srcPosition, srcVelocity, -perpDir, speed, childSize);
    }

    [BurstCompile]
    private void SpawnChildAsteroid(ref EntityCommandBuffer ecb, float3 parentPos, float3 parentVelocity, float3 dirOffset, float parentSpeed, int childSize)
    {
        float3 velocityDir = parentVelocity + dirOffset;
        velocityDir = math.normalize(velocityDir);
        float childSpeed = parentSpeed * spawnRO.GetRandomChildAsteroidSpeedMultiplier();
        SpawnAsteroid(ref ecb, parentPos + velocityDir * childSize, velocityDir, childSpeed, childSize);
    }

    [BurstCompile]
    private void SpawnAsteroid(ref EntityCommandBuffer ecb, float3 pos, float3 velocityDir, float speed, int asteroidSize)
    {
        Entity newAsteroid = ecb.Instantiate(spawnRO.AsteroidPrefab);
        ecb.SetComponent(newAsteroid, new LocalTransform { Position = pos, Rotation = spawnRO.GetRandomRotation(), Scale = asteroidSize });
        ecb.SetComponent(newAsteroid, new MovementData { acceleration = float3.zero, velocity = velocityDir * speed, angularVelocity = spawnRO.GetRandomAsteroidAngularVelocity(), maxSpeed = speed });
        ecb.SetComponent(newAsteroid, new SphereColliderData { radius = asteroidSize/2 });
        ecb.SetComponent(newAsteroid, new AsteroidData { size = asteroidSize });
        ecb.SetComponent(newAsteroid, new EnemyData { destroyed = false, score = asteroidSize * spawnRO.AsteroidBaseScore });
    }

    [BurstCompile]
    private void SpawnUFO(ref EntityCommandBuffer ecb, float3 pos)
    {
        Entity newUFO = ecb.Instantiate(spawnRO.UFOPrefab);
        ecb.SetComponent(newUFO, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = 1 });
    }

    [BurstCompile]
    private void SpawnPowerUp(ref EntityCommandBuffer ecb, Entity prefab, float3 pos)
    {
        Entity newPowerUp = ecb.Instantiate(prefab);
        ecb.SetComponent(newPowerUp, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = 1 });
    }
}
