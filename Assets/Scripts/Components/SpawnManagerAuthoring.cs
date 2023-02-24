using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnManagerAuthoring : MonoBehaviour
{
    public float minAsteroidSpawnDelay = 10;
    public float maxAsteroidSpawnDelay = 20;
    public float minAsteroidSpeed = 2;
    public float maxAsteroidSpeed = 10;
    public float minAsteroidSplitSpeedMultiplier = 0.5f;
    public float maxAsteroidSplitSpeedMultiplier = 1.5f;
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

    public class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var data = new SpawnManagerData
            {
                minAsteroidSpawnDelay = authoring.minAsteroidSpawnDelay,
                maxAsteroidSpawnDelay = authoring.maxAsteroidSpawnDelay,
                minAsteroidSpeed = authoring.minAsteroidSpeed,
                maxAsteroidSpeed = authoring.maxAsteroidSpeed,
                asteroidSpawnSize = authoring.asteroidSpawnSize,
                minAsteroidSize = authoring.minAsteroidSize,
                minAsteroidSplitSpeedMultiplier = authoring.minAsteroidSplitSpeedMultiplier,
                maxAsteroidSplitSpeedMultiplier = authoring.maxAsteroidSplitSpeedMultiplier,
                asteroidBaseScore = authoring.asteroidBaseScore,
                minUFOSpawnDelay = authoring.minUFOSpawnDelay,
                maxUFOSpawnDelay = authoring.maxUFOSpawnDelay,
                minPowerUpDelay = authoring.minPowerUpDelay,
                maxPowerUpDelay = authoring.maxPowerUpDelay,
                initialAsteroidsCount = authoring.initialAsteroidsCount,
                sqrSafetyRadius = authoring.safetyRadius * authoring.safetyRadius,
                nextAsteroidSpawnTick = 0,
                nextUFOSpawnTick = 0,
                asteroidPrefab = GetEntity(authoring.asteroidPrefab),
                ufoPrefab = GetEntity(authoring.ufoPrefab),
                multiShootPrefab = GetEntity(authoring.multiShootPrefab),
                shieldPrefab = GetEntity(authoring.shieldPrefab),
                initialSpawnProcessed = false
            };
            AddComponent(data);
        }
    }
}

struct SpawnManagerData : IComponentData
{
    public float minAsteroidSpawnDelay;
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

    public float sqrSafetyRadius;

    public Entity asteroidPrefab;
    public Entity ufoPrefab;
    public Entity multiShootPrefab;
    public Entity shieldPrefab;

    public bool initialSpawnProcessed;
}
