using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct SpawnAspect : IAspect
{
    public readonly Entity entity;

    private readonly RefRO<SpawnDesignData> designData;
    private readonly RefRO<SpawnAsteroidDesignData> asteroidDesignData;
    private readonly RefRO<SpawnUFODesignData> ufoDesignData;
    private readonly RefRO<SpawnPowerUpDesignData> powerUpDesignData;
    private readonly RefRW<SpawnRuntimeData> runtimeData;

    public int InitialAsteroidsToSpawn => asteroidDesignData.ValueRO.initialAsteroidsCount;
    public int AsteroidSpawnSize => asteroidDesignData.ValueRO.asteroidSpawnSize;
    public int AsteroidBaseScore => asteroidDesignData.ValueRO.asteroidBaseScore;

    public Entity AsteroidPrefab => asteroidDesignData.ValueRO.asteroidPrefab;
    public Entity UFOPrefab => ufoDesignData.ValueRO.ufoPrefab;
    public Entity ShieldPowerUpPrefab => powerUpDesignData.ValueRO.shieldPrefab;
    public Entity MultiShootPowerUpPrefab => powerUpDesignData.ValueRO.multiShootPrefab;

    public void ResetRuntimeData()
    {
        runtimeData.ValueRW.nextAsteroidSpawnTick = 0;
        runtimeData.ValueRW.nextUFOSpawnTick = 0;
        runtimeData.ValueRW.nextPowerUpSpawnTick = 0;
        runtimeData.ValueRW.initialSpawnProcessed = false;
    }

    public bool NeedProcessInitialSpawn()
    {
        return !runtimeData.ValueRO.initialSpawnProcessed;
    }

    public void SetInitialSpawnProcessed(bool processed)
    {
        runtimeData.ValueRW.initialSpawnProcessed = processed;
    }

    public void ComputeNextAsteroidSpawn(double currentTime)
    {
        runtimeData.ValueRW.nextAsteroidSpawnTick = currentTime + UnityEngine.Random.Range(asteroidDesignData.ValueRO.minAsteroidSpawnDelay, asteroidDesignData.ValueRO.maxAsteroidSpawnDelay);
    }

    public void ComputeNextUFOSpawn(double currentTime)
    {
        runtimeData.ValueRW.nextUFOSpawnTick = currentTime + UnityEngine.Random.Range(ufoDesignData.ValueRO.minUFOSpawnDelay, ufoDesignData.ValueRO.maxUFOSpawnDelay);
    }

    public void ComputeNextPowerUpSpawn(double currentTime)
    {
        runtimeData.ValueRW.nextPowerUpSpawnTick = currentTime + UnityEngine.Random.Range(powerUpDesignData.ValueRO.minPowerUpDelay, powerUpDesignData.ValueRO.maxPowerUpDelay);
    }

    public float GetRandomAsteroidSpeed()
    {
        return UnityEngine.Random.Range(asteroidDesignData.ValueRO.minAsteroidSpeed, asteroidDesignData.ValueRO.maxAsteroidSpeed);
    }

    public float GetRandomAsteroidAngularVelocity()
    {
        return UnityEngine.Random.Range(asteroidDesignData.ValueRO.minAsteroidAngularSpeed, asteroidDesignData.ValueRO.maxAsteroidAngularSpeed);
    }

    public float GetRandomChildAsteroidSpeedMultiplier()
    {
        return UnityEngine.Random.Range(asteroidDesignData.ValueRO.minAsteroidSplitSpeedMultiplier, asteroidDesignData.ValueRO.maxAsteroidSplitSpeedMultiplier);
    }

    public bool CanSplitAsteroid(int asteroidSize)
    {
        return asteroidSize > asteroidDesignData.ValueRO.minAsteroidSize;
    }

    public bool NeedToSpawnAsteroid(double currentTime)
    {
        return currentTime > runtimeData.ValueRO.nextAsteroidSpawnTick;
    }

    public bool NeedToSpawnUFO(double currentTime)
    {
        return currentTime > runtimeData.ValueRO.nextUFOSpawnTick;
    }

    public bool NeedToSpawnPowerUp(double currentTime)
    {
        return currentTime > runtimeData.ValueRO.nextPowerUpSpawnTick;
    }

    public float3 GetRandomEnemySpawnPos(Vector3 safetyCenter)
    {
        float3 spawnPos;

        do
        {
            spawnPos = GetRandomSpawnPos();
        } while (math.distancesq(spawnPos, safetyCenter) <= designData.ValueRO.sqrSafetyRadius);

        return spawnPos;
    }

    public float3 GetRandomSpawnPos()
    {
        float3 spawnPos;
        float arenaHalfWidth = Game.instance.arenaWidth * 0.5f;
        float arenaHalfHeight = Game.instance.arenaHeight * 0.5f;
        spawnPos.x = UnityEngine.Random.Range(-arenaHalfWidth, arenaHalfWidth);
        spawnPos.y = UnityEngine.Random.Range(-arenaHalfHeight, arenaHalfHeight);
        spawnPos.z = 0;
        return spawnPos;
    }

    public float3 GetRandomDirection()
    {
        Vector3 dir = UnityEngine.Random.onUnitSphere;
        dir.z = 0;
        dir.Normalize();
        return dir;
    }

    public quaternion GetRandomRotation()
    {
        float3 euler;
        euler.x = UnityEngine.Random.Range(-180, 180);
        euler.y = UnityEngine.Random.Range(-180, 180);
        euler.z = UnityEngine.Random.Range(-180, 180);
        return quaternion.Euler(euler);
    }

    public PowerUpType GetRandomPowerUpType()
    {
        return (PowerUpType)UnityEngine.Random.Range(0, (int)PowerUpType.Count);
    }
}
