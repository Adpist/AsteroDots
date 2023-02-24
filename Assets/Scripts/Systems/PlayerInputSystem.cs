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
                float bulletMaxSpeed = movement.maxSpeed + playerStats.bulletSpeed;
                Vector3 bulletVelocity = movement.velocity + transform.Up() * playerStats.bulletSpeed;
                SpawnBullet(ref ecb, playerStats.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, 0, bulletMaxSpeed, playerStats.bulletLifeTime);
                if (SystemAPI.Time.ElapsedTime < playerStats.multiShootExpireTick)
                {
                    SpawnBullet(ref ecb, playerStats.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, -45, bulletMaxSpeed, playerStats.bulletLifeTime);
                    SpawnBullet(ref ecb, playerStats.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, 45, bulletMaxSpeed, playerStats.bulletLifeTime);
                }
            }
        
            ecb.Playback(state.EntityManager);
        }
    }

    void SpawnBullet(ref EntityCommandBuffer ecb, Entity prefab, Vector3 playerPos, Vector3 up, Vector3 velocity, float rotation, float maxSpeed, float lifeTime)
    {
        Quaternion rotationQuat = Quaternion.Euler(0, 0, rotation);
        Vector3 spawnPos = playerPos + rotationQuat * up;
        if (rotation != 0)
        {
            velocity = rotationQuat *velocity;
        }
        Entity newBullet = ecb.Instantiate(prefab);
        ecb.SetComponent(newBullet, new LocalTransform { Position = spawnPos, Rotation = Quaternion.identity, Scale = 1 });
        ecb.SetComponent(newBullet, new Movement { acceleration = Vector3.zero, velocity = velocity, angularVelocity = 0, maxSpeed = maxSpeed });
        ecb.SetComponent(newBullet, new BulletStats { lifeTime = lifeTime });
    }

    void RestartGame(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(AsteroidStats)).ToEntityArray(Allocator.Temp));
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(BulletStats)).ToEntityArray(Allocator.Temp));
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(UFOData)).ToEntityArray(Allocator.Temp));
        state.EntityManager.DestroyEntity(state.EntityManager.CreateEntityQuery(typeof(PowerUpData)).ToEntityArray(Allocator.Temp));

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
