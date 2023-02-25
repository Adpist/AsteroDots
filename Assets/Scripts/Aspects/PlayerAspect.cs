using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity entity;

    private readonly TransformAspect transform;

    private readonly RefRO<PlayerDesignData> designData;
    private readonly RefRW<PlayerRuntimeData> runtimeData;
    private readonly RefRW<MovementData> movementData;

    public bool IsDead => runtimeData.ValueRO.dead;
    public float RotationSpeed => designData.ValueRO.rotationSpeed;
    public float AccelerationSpeed => designData.ValueRO.accelerationSpeed;
    public float MaxSpeed => movementData.ValueRO.maxSpeed;
    public float BulletSpeed => designData.ValueRO.bulletSpeed;
    public float BulletMaxSpeed => MaxSpeed + BulletSpeed;
    public float BulletLifeTime => designData.ValueRO.bulletLifeTime;
    public float MultiBulletAngle => designData.ValueRO.multiBulletAngle;

    public float3 WorldPosition => transform.WorldPosition;
    public float3 Up => transform.Up;

    public Entity BulletPrefab => designData.ValueRO.bulletPrefab;

    public void ResetRuntimeData()
    {
        runtimeData.ValueRW.dead = false;
        runtimeData.ValueRW.multiShootExpireTick = 0;
        runtimeData.ValueRW.multiShootExpireTick = 0;
        transform.WorldPosition = float3.zero;
        transform.WorldRotation = quaternion.identity;
        movementData.ValueRW.acceleration = float3.zero;
        movementData.ValueRW.velocity = float3.zero;
        movementData.ValueRW.angularVelocity = 0;
    }

    public void SetDead(bool dead)
    {
        runtimeData.ValueRW.dead = dead;
    }

    public void SetAngularVelocity(float angularVelocity)
    {
        movementData.ValueRW.angularVelocity = angularVelocity;
    }

    public void SetAcceleration(float3 acceleration)
    {
        movementData.ValueRW.acceleration = acceleration;
    }

    public float3 GetNewBulletVelocity()
    {
        return movementData.ValueRO.velocity + Up * BulletSpeed;
    }

    public bool IsMultiShootActive(double currentTime)
    {
        return currentTime < runtimeData.ValueRO.multiShootExpireTick;
    }

    public bool IsInvulnerable(double currentTime)
    {
        return IsDead || currentTime < runtimeData.ValueRO.invulnerabilityExpireTick;
    }

    public void PickupPowerUp(PowerUpType powerUpType, double currentTime, double duration)
    {
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                runtimeData.ValueRW.invulnerabilityExpireTick = currentTime + duration;
                break;

            case PowerUpType.MultiShoot:
                runtimeData.ValueRW.multiShootExpireTick = currentTime + duration;
                break;
        }
    }
}
