using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum UFOState
{
    Wander,
    Chase
}

public class UFOAuthoring : MonoBehaviour
{
    public float chaseRadius = 20;
    public float wanderAcceleration = 10;
    public float wanderMaxSpeed = 5;
    public float chaseAcceleration = 30;
    public float chaseMaxSpeed = 15;
    public float minWanderDirectionChangeDelay = 5;
    public float maxWanderDirectionChangeDelay = 10;

    public class Baker : Baker<UFOAuthoring>
    {
        public override void Bake(UFOAuthoring authoring)
        {
            var data = new UFOData
            {
                state = UFOState.Wander,
                chaseRadius = authoring.chaseRadius,
                wanderMaxSpeed = authoring.wanderMaxSpeed,
                wanderAcceleration = authoring.wanderAcceleration,
                chaseAcceleration = authoring.chaseAcceleration,
                chaseMaxSpeed = authoring.chaseMaxSpeed,
                minWanderDirectionChangeDelay = authoring.minWanderDirectionChangeDelay,
                maxWanderDirectionChangeDelay = authoring.maxWanderDirectionChangeDelay,
                nextDirectionChangeTick = 0,
                destroyed = false
            };
            AddComponent(data);
        }
    }
}

struct UFOData : IComponentData
{
    public UFOState state;
    public float chaseRadius;
    public float wanderAcceleration;
    public float wanderMaxSpeed;
    public float chaseAcceleration;
    public float chaseMaxSpeed;
    public float minWanderDirectionChangeDelay;
    public float maxWanderDirectionChangeDelay;
    public float nextDirectionChangeTick;
    public bool destroyed;
}
