using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct PlayerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
        PlayerData playerData = state.EntityManager.GetComponentData<PlayerData>(playerEntity);

        if (playerData.dead)
        {
            RestartGame(ref state);
        }
        else
        {
            HandleMovementInputs(ref state, ref playerEntity);
            HandleShooting(ref state, ref playerEntity);
        }
    }

    [BurstCompile]
    void HandleMovementInputs(ref SystemState state, ref Entity player)
    {
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool accelerating = Input.GetKey(KeyCode.UpArrow);

        PlayerData playerData = state.EntityManager.GetComponentData<PlayerData>(player);
        LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(player);
        MovementData movement = state.EntityManager.GetComponentData<MovementData>(player);

        float angularSpeed = 0;
        Vector3 acceleration = Vector3.zero;
        if (left)
        {
            angularSpeed += playerData.rotationSpeed;
        }

        if (right)
        {
            angularSpeed -= playerData.rotationSpeed;
        }

        if (accelerating)
        {
            acceleration = transform.Up();
            acceleration *= playerData.accelerationSpeed;
        }

        movement.angularVelocity = angularSpeed;
        movement.acceleration = acceleration;
        state.EntityManager.SetComponentData(player, movement);
    }

    [BurstCompile]
    void HandleShooting(ref SystemState state, ref Entity player)
    {
        bool shooting = Input.GetKeyDown(KeyCode.Space);

        PlayerData playerData = state.EntityManager.GetComponentData<PlayerData>(player);
        LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(player);
        MovementData movement = state.EntityManager.GetComponentData<MovementData>(player);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (shooting)
        {
            float bulletMaxSpeed = movement.maxSpeed + playerData.bulletSpeed;
            Vector3 bulletVelocity = movement.velocity + transform.Up() * playerData.bulletSpeed;
            SpawnBullet(ref ecb, playerData.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, 0, bulletMaxSpeed, playerData.bulletLifeTime);
            if (SystemAPI.Time.ElapsedTime < playerData.multiShootExpireTick)
            {
                SpawnBullet(ref ecb, playerData.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, -playerData.multiBulletAngle, bulletMaxSpeed, playerData.bulletLifeTime);
                SpawnBullet(ref ecb, playerData.bulletPrefab, transform.Position, transform.Up(), bulletVelocity, playerData.multiBulletAngle, bulletMaxSpeed, playerData.bulletLifeTime);
            }
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
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
        ecb.SetComponent(newBullet, new MovementData { acceleration = Vector3.zero, velocity = velocity, angularVelocity = 0, maxSpeed = maxSpeed });
        ecb.SetComponent(newBullet, new BulletData { lifeTime = lifeTime });
    }

    [BurstCompile]
    void RestartGame(ref SystemState state)
    {
        DestroyAll<EnemyData>(ref state);
        DestroyAll<BulletData>(ref state);
        DestroyAll<PowerUpData>(ref state);
        
        ResetPlayer(ref state);
        SpawnAspect spawn = SystemAPI.GetAspectRW<SpawnAspect>(SystemAPI.GetSingletonEntity<SpawnDesignData>());
        spawn.ResetRuntimeData();
        Game.instance.ResetScore();
    }

    [BurstCompile]
    void DestroyAll<T>(ref SystemState state) where T : IComponentData
    {
        NativeArray<Entity> entities = state.EntityManager.CreateEntityQuery(typeof(T)).ToEntityArray(Allocator.Temp);
        state.EntityManager.DestroyEntity(entities);
        entities.Dispose();
    }

    [BurstCompile]
    void ResetPlayer(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
        PlayerData playerData = state.EntityManager.GetComponentData<PlayerData>(playerEntity);
        LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
        MovementData movement = state.EntityManager.GetComponentData<MovementData>(playerEntity);
        playerData.dead = false;
        transform.Position = Vector3.zero;
        transform.Rotation = Quaternion.identity;
        movement.acceleration = Vector3.zero;
        movement.velocity = Vector3.zero;
        movement.angularVelocity = 0;
        state.EntityManager.SetComponentData<PlayerData>(playerEntity, playerData);
        state.EntityManager.SetComponentData<LocalTransform>(playerEntity, transform);
        state.EntityManager.SetComponentData<MovementData>(playerEntity, movement);
    }
}
