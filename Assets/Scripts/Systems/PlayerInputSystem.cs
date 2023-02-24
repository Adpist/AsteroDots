using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct PlayerInputSystem : ISystem
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
        float deltaTime = SystemAPI.Time.DeltaTime;

        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool accelerating = Input.GetKey(KeyCode.UpArrow);
        bool shooting = Input.GetKeyDown(KeyCode.Space);

        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerStats>();
        PlayerStats playerStats = state.EntityManager.GetComponentData<PlayerStats>(playerEntity);
        LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
        Movement movement = state.EntityManager.GetComponentData<Movement>(playerEntity);

        if (playerStats.dead)
        {
            RestartGame(ref state);
        }
        else
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        
            float angularSpeed = 0;
            Vector3 acceleration = Vector3.zero;
            if (left)
            {
                angularSpeed += playerStats.rotationSpeed;
            }

            if (right)
            {
                angularSpeed -= playerStats.rotationSpeed;
            }

            if (accelerating)
            {
                acceleration = transform.Up();
                acceleration *= playerStats.accelerationSpeed;
            }

            movement.angularVelocity = angularSpeed;
            movement.acceleration = acceleration;
            state.EntityManager.SetComponentData<Movement>(playerEntity, movement);

            if (shooting)
            {
                Entity newBullet = ecb.Instantiate(playerStats.bulletPrefab);
                Vector3 spawnPos = transform.Position + transform.Up();
                Vector3 bulletVelocity = movement.velocity + transform.Up() * playerStats.bulletSpeed;
                ecb.SetComponent(newBullet, new LocalTransform { Position = spawnPos, Rotation = Quaternion.identity, Scale = 1 });
                ecb.SetComponent(newBullet, new Movement { acceleration = Vector3.zero, velocity = bulletVelocity, angularVelocity = 0, maxSpeed = movement.maxSpeed + playerStats.bulletSpeed });
                ecb.SetComponent(newBullet, new BulletStats { lifeTime = playerStats.bulletLifeTime });
            }
        
            ecb.Playback(state.EntityManager);
        }
    }

    void RestartGame(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(AsteroidStats)));
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(BulletStats)));
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(UFOData)).ToEntityArray(Allocator.Temp));

        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerStats>();
        PlayerStats playerStats = state.EntityManager.GetComponentData<PlayerStats>(playerEntity);
        LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
        Movement movement = state.EntityManager.GetComponentData<Movement>(playerEntity);
        playerStats.dead = false;
        transform.Position = Vector3.zero;
        transform.Rotation = Quaternion.identity;
        movement.acceleration = Vector3.zero;
        movement.velocity = Vector3.zero;
        movement.angularVelocity = 0;
        state.EntityManager.SetComponentData<PlayerStats>(playerEntity, playerStats);
        state.EntityManager.SetComponentData<LocalTransform>(playerEntity, transform);
        state.EntityManager.SetComponentData<Movement>(playerEntity, movement);

        Entity spawnManager = SystemAPI.GetSingletonEntity<SpawnManagerData>();
        SpawnManagerData spawnData = state.EntityManager.GetComponentData<SpawnManagerData>(spawnManager);
        spawnData.nextAsteroidSpawnTick = 0;
        spawnData.nextUFOSpawnTick = 0;
        spawnData.initialSpawnProcessed = false;
        state.EntityManager.SetComponentData(spawnManager, spawnData);

        Game.instance.ResetScore();
    }
}
