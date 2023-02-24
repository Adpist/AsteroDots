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

    public float minUFOSpawnDelay = 15;
    public float maxUFOSpawnDelay = 30;

    public int initialAsteroidsCount = 5;
    public float safetyRadius = 15;
    public GameObject asteroidPrefab;
    public GameObject ufoPrefab;

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
                minUFOSpawnDelay = authoring.minUFOSpawnDelay,
                maxUFOSpawnDelay = authoring.maxUFOSpawnDelay,
                initialAsteroidsCount = authoring.initialAsteroidsCount,
                sqrSafetyRadius = authoring.safetyRadius * authoring.safetyRadius,
                nextAsteroidSpawnTick = 0,
                nextUFOSpawnTick = 0,
                asteroidPrefab = GetEntity(authoring.asteroidPrefab),
                ufoPrefab = GetEntity(authoring.ufoPrefab),
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
    public float minAsteroidSpeed;
    public float maxAsteroidSpeed;
    public int initialAsteroidsCount;
    public float minUFOSpawnDelay;
    public float maxUFOSpawnDelay;
    public float sqrSafetyRadius;
    public double nextAsteroidSpawnTick;
    public double nextUFOSpawnTick;
    public Entity asteroidPrefab;
    public Entity ufoPrefab;
    public bool initialSpawnProcessed;
}
