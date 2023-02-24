using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;

public class MovementAuthoring : MonoBehaviour
{
    public float3 acceleration;
    public float3 velocity;
    public float maxSpeed;

    public class Baker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            var data = new MovementData
            {
                acceleration = authoring.acceleration,
                velocity = authoring.velocity,
                maxSpeed = authoring.maxSpeed,
                angularVelocity = 0,
            };
            AddComponent(data);
        }
    }
}

struct MovementData : IComponentData
{
    public float3 acceleration;
    public float3 velocity;
    public float angularVelocity;
    public float maxSpeed;
}
