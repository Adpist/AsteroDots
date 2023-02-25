using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct PlayerSystem : ISystem
{
    PlayerAspect playerRW;
    PlayerAspect playerRO;
    bool needResetGame;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerDesignData>();
        state.RequireForUpdate<PlayerRuntimeData>();
        needResetGame = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerDesignData>();
        playerRW = SystemAPI.GetAspectRW<PlayerAspect>(playerEntity);
        playerRO = SystemAPI.GetAspectRO<PlayerAspect>(playerEntity);

        //Couldn't set dead value to false through player aspect inside ResetGame
        //So reset the game in the update after the death detection
        if (needResetGame)
        {
            ResetGame(ref state);
            needResetGame = false;
        }
        else if (playerRO.IsDead)
        {
            needResetGame = true;
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

        float angularSpeed = 0;
        Vector3 acceleration = Vector3.zero;
        if (left)
        {
            angularSpeed += playerRO.RotationSpeed;
        }

        if (right)
        {
            angularSpeed -= playerRO.RotationSpeed;
        }

        if (accelerating)
        {
            acceleration = playerRO.Up;
            acceleration *= playerRO.AccelerationSpeed;
        }

        playerRW.SetAcceleration(acceleration);
        playerRW.SetAngularVelocity(angularSpeed);
    }

    [BurstCompile]
    void HandleShooting(ref SystemState state, ref Entity player)
    {
        bool shooting = Input.GetKeyDown(KeyCode.Space);

        if (shooting)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            float bulletMaxSpeed = playerRO.BulletMaxSpeed;
            Vector3 bulletVelocity = playerRO.GetNewBulletVelocity();
            SpawnBullet(ref state, ref ecb, playerRO.WorldPosition, playerRO.Up, bulletVelocity, 0, bulletMaxSpeed, playerRO.BulletLifeTime);
            if (playerRO.IsMultiShootActive(SystemAPI.Time.ElapsedTime))
            {
                SpawnBullet(ref state, ref ecb, playerRO.WorldPosition, playerRO.Up, bulletVelocity, -playerRO.MultiBulletAngle, bulletMaxSpeed, playerRO.BulletLifeTime);
                SpawnBullet(ref state, ref ecb, playerRO.WorldPosition, playerRO.Up, bulletVelocity, playerRO.MultiBulletAngle, bulletMaxSpeed, playerRO.BulletLifeTime);
            }
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    void SpawnBullet(ref SystemState state, ref EntityCommandBuffer ecb, Vector3 playerPos, Vector3 up, Vector3 velocity, float rotation, float maxSpeed, float lifeTime)
    {
        Quaternion rotationQuat = Quaternion.Euler(0, 0, rotation);
        Vector3 spawnPos = playerPos + rotationQuat * up;
        if (rotation != 0)
        {
            velocity = rotationQuat *velocity;
        }
        Entity newBullet = ecb.Instantiate(playerRO.BulletPrefab);
        ecb.SetComponent(newBullet, new LocalTransform { Position = spawnPos, Rotation = Quaternion.identity, Scale = 1 });
        ecb.SetComponent(newBullet, new MovementData { acceleration = Vector3.zero, velocity = velocity, angularVelocity = 0, maxSpeed = maxSpeed });
        ecb.SetComponent(newBullet, new LifeTimeData { expirationTick = SystemAPI.Time.ElapsedTime + lifeTime });
        ecb.SetComponent(newBullet, new BulletTag { });
    }

    [BurstCompile]
    void ResetGame(ref SystemState state)
    {
        DestroyAll<EnemyData>(ref state);
        DestroyAll<BulletTag>(ref state);
        DestroyAll<PowerUpData>(ref state);

        PlayerAspect player = SystemAPI.GetAspectRW<PlayerAspect>(SystemAPI.GetSingletonEntity<PlayerDesignData>());
        player.ResetRuntimeData();
        SpawnAspect spawn = SystemAPI.GetAspectRW<SpawnAspect>(SystemAPI.GetSingletonEntity<SpawnDesignData>());
        spawn.ResetRuntimeData();
        Game.instance.ResetScore();
    }

    void DestroyAll<T>(ref SystemState state) where T : IComponentData
    {
        NativeArray<Entity> entities = state.EntityManager.CreateEntityQuery(typeof(T)).ToEntityArray(Allocator.Temp);
        state.EntityManager.DestroyEntity(entities);
        entities.Dispose();
    }
}
