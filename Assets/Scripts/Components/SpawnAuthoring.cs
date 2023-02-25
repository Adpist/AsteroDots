using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnAuthoring : MonoBehaviour
{
    public float minAsteroidSpawnDelay = 10;
    public float maxAsteroidSpawnDelay = 20;
    public float minAsteroidSpeed = 2;
    public float maxAsteroidSpeed = 10;
    public float minAsteroidSplitSpeedMultiplier = 0.5f;
    public float maxAsteroidSplitSpeedMultiplier = 1.5f;
    public float minAsteroidAngularSpeed = -180.0f;
    public float maxAsteroidAngularSpeed = 180.0f;
    public int asteroidSpawnSize = 8;
    public int minAsteroidSize = 2;
    public int asteroidBaseScore = 10;
    public int initialAsteroidsCount = 5;

    public float minUFOSpawnDelay = 15;
    public float maxUFOSpawnDelay = 30;

    public float minPowerUpDelay = 15;
    public float maxPowerUpDelay = 30;

    public float safetyRadius = 15;

    public GameObject asteroidPrefab;
    public GameObject ufoPrefab;
    public GameObject multiShootPrefab;
    public GameObject shieldPrefab;

    public class Baker : Baker<SpawnAuthoring>
    {
        public override void Bake(SpawnAuthoring authoring)
        {
            SpawnDesignData spawnDesign = new SpawnDesignData
            {
                sqrSafetyRadius = authoring.safetyRadius * authoring.safetyRadius
            };

            SpawnAsteroidDesignData asteroidDesign = new SpawnAsteroidDesignData
            {
                minAsteroidSpawnDelay = authoring.minAsteroidSpawnDelay,
                maxAsteroidSpawnDelay = authoring.maxAsteroidSpawnDelay,
                minAsteroidSpeed = authoring.minAsteroidSpeed,
                maxAsteroidSpeed = authoring.maxAsteroidSpeed,
                minAsteroidSplitSpeedMultiplier = authoring.minAsteroidSplitSpeedMultiplier,
                maxAsteroidSplitSpeedMultiplier = authoring.maxAsteroidSplitSpeedMultiplier,
                minAsteroidAngularSpeed = authoring.minAsteroidAngularSpeed,
                maxAsteroidAngularSpeed = authoring.maxAsteroidAngularSpeed,
                asteroidSpawnSize = authoring.asteroidSpawnSize,
                minAsteroidSize = authoring.minAsteroidSize,
                asteroidBaseScore = authoring.asteroidBaseScore,
                initialAsteroidsCount = authoring.initialAsteroidsCount,
                asteroidPrefab = GetEntity(authoring.asteroidPrefab)
            };

            SpawnUFODesignData ufoDesign = new SpawnUFODesignData
            {
                minUFOSpawnDelay = authoring.minUFOSpawnDelay,
                maxUFOSpawnDelay = authoring.maxUFOSpawnDelay,
                ufoPrefab = GetEntity(authoring.ufoPrefab)
            };

            SpawnPowerUpDesignData powerUpDesign = new SpawnPowerUpDesignData
            {
                minPowerUpDelay = authoring.minPowerUpDelay,
                maxPowerUpDelay = authoring.maxPowerUpDelay,
                multiShootPrefab = GetEntity(authoring.multiShootPrefab),
                shieldPrefab = GetEntity(authoring.shieldPrefab)
            };

            SpawnRuntimeData spawnRuntime = new SpawnRuntimeData
            {
                nextUFOSpawnTick = 0,
                nextPowerUpSpawnTick = 0,
                nextAsteroidSpawnTick = 0,
                initialSpawnProcessed = false
            };

            AddComponent(spawnDesign);
            AddComponent(asteroidDesign);
            AddComponent(ufoDesign);
            AddComponent(powerUpDesign);
            AddComponent(spawnRuntime);
        }
    }
}

struct SpawnAsteroidDesignData : IComponentData
{
    public float minAsteroidSpawnDelay;
    public float maxAsteroidSpawnDelay;
    public float minAsteroidSpeed;
    public float maxAsteroidSpeed;
    public float minAsteroidSplitSpeedMultiplier;
    public float maxAsteroidSplitSpeedMultiplier;
    public float minAsteroidAngularSpeed;
    public float maxAsteroidAngularSpeed;
    public int asteroidSpawnSize;
    public int minAsteroidSize;
    public int initialAsteroidsCount;
    public int asteroidBaseScore;
    public Entity asteroidPrefab;
}

struct SpawnUFODesignData : IComponentData
{
    public float minUFOSpawnDelay;
    public float maxUFOSpawnDelay;
    public Entity ufoPrefab;
}

struct SpawnPowerUpDesignData : IComponentData
{
    public float minPowerUpDelay;
    public float maxPowerUpDelay;
    public Entity multiShootPrefab;
    public Entity shieldPrefab;
}

struct SpawnDesignData : IComponentData
{
    public float sqrSafetyRadius;

    /*public float minAsteroidSpawnDelay;
    public float maxAsteroidSpawnDelay;
    public double nextAsteroidSpawnTick;
    public float minAsteroidSpeed;
    public float maxAsteroidSpeed;
    public float minAsteroidSplitSpeedMultiplier;
    public float maxAsteroidSplitSpeedMultiplier;
    public int asteroidSpawnSize;
    public int minAsteroidSize;
    public int initialAsteroidsCount;
    public int asteroidBaseScore;

    public float minUFOSpawnDelay;
    public float maxUFOSpawnDelay;
    public double nextUFOSpawnTick;

    public float minPowerUpDelay;
    public float maxPowerUpDelay;
    public double nextPowerUpSpawnTick;

    public Entity asteroidPrefab;
    public Entity ufoPrefab;
    public Entity multiShootPrefab;
    public Entity shieldPrefab;

    public bool initialSpawnProcessed;*/
}

struct SpawnRuntimeData : IComponentData
{
    public double nextUFOSpawnTick;
    public double nextPowerUpSpawnTick;
    public double nextAsteroidSpawnTick;
    public bool initialSpawnProcessed;
}