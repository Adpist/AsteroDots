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
            ProcessInitialSpawn(ref state, ref ecb, ref spawnManager, ref spawnData);
        }
        else
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
            LocalTransform playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);

            UpdateAsteroidSpawn(ref state, ref ecb, ref spawnManager, ref spawnData, playerTransform.Position);
            UpdateUFOSpawn(ref state, ref ecb, ref spawnManager, ref spawnData, playerTransform.Position);
            UpdatePowerUpSpawn(ref state, ref ecb, ref spawnManager, ref spawnData);

            NativeArray<Entity> asteroids = state.EntityManager.CreateEntityQuery(typeof(AsteroidData)).ToEntityArray(Allocator.Temp);
            foreach(Entity asteroid in asteroids)
            {
                AsteroidData asteroidData = state.EntityManager.GetComponentData<AsteroidData>(asteroid);
                EnemyData enemyData = state.EntityManager.GetComponentData<EnemyData>(asteroid);
                if (enemyData.destroyed)
                {
                    if (asteroidData.size > spawnData.minAsteroidSize)
                    {
                        SplitAsteroid(ref state, ref ecb, asteroid, ref asteroidData, ref spawnData);
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
    private void UpdateAsteroidSpawn(ref SystemState state, ref EntityCommandBuffer ecb, ref Entity spawnManager, ref SpawnManagerData spawnData, Vector3 playerPosition)
    {
        if (SystemAPI.Time.ElapsedTime > spawnData.nextAsteroidSpawnTick)
        {
            SpawnBigAsteroid(ref ecb, ref spawnData, playerPosition);
            spawnData.nextAsteroidSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minAsteroidSpawnDelay, spawnData.maxAsteroidSpawnDelay);
            state.EntityManager.SetComponentData(spawnManager, spawnData);
        }
    }

    [BurstCompile]
    private void UpdateUFOSpawn(ref SystemState state, ref EntityCommandBuffer ecb, ref Entity spawnManager, ref SpawnManagerData spawnData, Vector3 playerPosition)
    {
        if (SystemAPI.Time.ElapsedTime > spawnData.nextUFOSpawnTick)
        {
            SpawnUFO(ref ecb, spawnData.ufoPrefab, GetEnnemySpawnPos(playerPosition, spawnData.sqrSafetyRadius));
            spawnData.nextUFOSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minUFOSpawnDelay, spawnData.maxUFOSpawnDelay);
            state.EntityManager.SetComponentData(spawnManager, spawnData);
        }
    }

    [BurstCompile]
    private void UpdatePowerUpSpawn(ref SystemState state, ref EntityCommandBuffer ecb, ref Entity spawnManager, ref SpawnManagerData spawnData)
    {
        if (SystemAPI.Time.ElapsedTime > spawnData.nextPowerUpSpawnTick)
        {
            PowerUpType type = (PowerUpType)UnityEngine.Random.Range(0, (int)PowerUpType.Count);
            switch (type)
            {
                case PowerUpType.Shield:
                    SpawnPowerUp(ref ecb, spawnData.shieldPrefab, GetRandomSpawnPos());
                    break;

                case PowerUpType.MultiShoot:
                    SpawnPowerUp(ref ecb, spawnData.multiShootPrefab, GetRandomSpawnPos());
                    break;
            }
            spawnData.nextPowerUpSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minPowerUpDelay, spawnData.maxPowerUpDelay);
            state.EntityManager.SetComponentData(spawnManager, spawnData);
        }
    }

    [BurstCompile]
    private void ProcessInitialSpawn(ref SystemState state, ref EntityCommandBuffer ecb, ref Entity spawnManager, ref SpawnManagerData spawnData)
    {
        for (int i = 0; i < spawnData.initialAsteroidsCount; ++i)
        {
            SpawnBigAsteroid(ref ecb, ref spawnData, Vector3.zero);
        }
        spawnData.nextAsteroidSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minAsteroidSpawnDelay, spawnData.maxAsteroidSpawnDelay);
        spawnData.nextUFOSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minUFOSpawnDelay, spawnData.maxUFOSpawnDelay);
        spawnData.nextPowerUpSpawnTick = SystemAPI.Time.ElapsedTime + UnityEngine.Random.Range(spawnData.minPowerUpDelay, spawnData.maxPowerUpDelay);
        spawnData.initialSpawnProcessed = true;
        state.EntityManager.SetComponentData(spawnManager, spawnData);
    }

    [BurstCompile]
    private void SpawnBigAsteroid(ref EntityCommandBuffer ecb, ref SpawnManagerData spawnData, Vector3 playerPosition)
    {
        float speed = UnityEngine.Random.Range(spawnData.minAsteroidSpeed, spawnData.maxAsteroidSpeed);
        Vector3 spawnPos = GetEnnemySpawnPos(playerPosition, spawnData.sqrSafetyRadius);
        SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, spawnPos, GetAsteroidVelocityDirection(), speed, spawnData.asteroidSpawnSize, spawnData.asteroidBaseScore);
    }

    [BurstCompile]
    private void SplitAsteroid(ref SystemState state, ref EntityCommandBuffer ecb, Entity srcAsteroid, ref AsteroidData srcAsteroidData, ref SpawnManagerData spawnData)
    {
        MovementData asteroidMovement = state.EntityManager.GetComponentData<MovementData>(srcAsteroid);
        Vector3 asteroidPos = state.EntityManager.GetComponentData<LocalTransform>(srcAsteroid).Position;
        Vector3 asteroidVelocity = asteroidMovement.velocity;
        float speed = asteroidVelocity.magnitude;
        asteroidVelocity /= speed;
        Vector3 perpDir = Vector3.Cross(asteroidVelocity, Vector3.forward);
        int childSize = srcAsteroidData.size / 2;

        SpawnChildAsteroid(ref ecb, ref spawnData, asteroidPos, asteroidVelocity, perpDir, speed, childSize);
        SpawnChildAsteroid(ref ecb, ref spawnData, asteroidPos, asteroidVelocity, -perpDir, speed, childSize);
    }

    [BurstCompile]
    private void SpawnChildAsteroid(ref EntityCommandBuffer ecb, ref SpawnManagerData spawnData, Vector3 parentPos, Vector3 parentVelocity, Vector3 dirOffset, float parentSpeed, int childSize)
    {
        Vector3 velocityDir = parentVelocity + dirOffset;
        velocityDir.Normalize();
        float childSpeed = parentSpeed * UnityEngine.Random.Range(spawnData.minAsteroidSplitSpeedMultiplier, spawnData.maxAsteroidSplitSpeedMultiplier);
        SpawnAsteroid(ref ecb, spawnData.asteroidPrefab, parentPos + velocityDir * childSize, velocityDir, childSpeed, childSize, spawnData.asteroidBaseScore);
    }

    [BurstCompile]
    private void SpawnAsteroid(ref EntityCommandBuffer ecb, Entity prefab, Vector3 pos, Vector3 velocityDir, float speed, int asteroidSize, int baseScore)
    {
        Entity newAsteroid = ecb.Instantiate(prefab);
        ecb.SetComponent(newAsteroid, new LocalTransform { Position = pos, Rotation = GetRandomRotation(), Scale = asteroidSize });
        ecb.SetComponent(newAsteroid, new MovementData { acceleration = Vector3.zero, velocity = velocityDir * speed, angularVelocity = 0, maxSpeed = speed });
        ecb.SetComponent(newAsteroid, new SphereColliderData { radius = asteroidSize/2 });
        ecb.SetComponent(newAsteroid, new AsteroidData { size = asteroidSize });
        ecb.SetComponent(newAsteroid, new EnemyData { destroyed = false, score = asteroidSize * baseScore });
    }

    [BurstCompile]
    private void SpawnUFO(ref EntityCommandBuffer ecb, Entity prefab, Vector3 pos)
    {
        Entity newUFO = ecb.Instantiate(prefab);
        ecb.SetComponent(newUFO, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = 1 });
    }

    [BurstCompile]
    private void SpawnPowerUp(ref EntityCommandBuffer ecb, Entity prefab, Vector3 pos)
    {
        Entity newPowerUp = ecb.Instantiate(prefab);
        ecb.SetComponent(newPowerUp, new LocalTransform { Position = pos, Rotation = Quaternion.identity, Scale = 1 });
    }

    [BurstCompile]
    public Vector3 GetEnnemySpawnPos(Vector3 safetyCenter, float sqrSafetyRadius)
    {
        Vector3 spawnPos = Vector3.zero;
        do
        {
            spawnPos = GetRandomSpawnPos();
        } while (math.distancesq(spawnPos, safetyCenter) <= sqrSafetyRadius);
        return spawnPos;
    }


    [BurstCompile]
    public Vector3 GetRandomSpawnPos()
    {
        Vector3 spawnPos = Vector3.zero;
        float arenaHalfWidth = Game.instance.arenaWidth * 0.5f;
        float arenaHalfHeight = Game.instance.arenaHeight * 0.5f;
        spawnPos.x = UnityEngine.Random.Range(-arenaHalfWidth, arenaHalfWidth);
        spawnPos.y = UnityEngine.Random.Range(-arenaHalfHeight, arenaHalfHeight);
        return spawnPos;
    }

    [BurstCompile]
    public Quaternion GetRandomRotation()
    {
        Vector3 euler = Vector3.zero;
        euler.x = UnityEngine.Random.Range(-180, 180);
        euler.y = UnityEngine.Random.Range(-180, 180);
        euler.z = UnityEngine.Random.Range(-180, 180);
        return Quaternion.Euler(euler);
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
