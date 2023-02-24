using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum PowerUpType
{
    Shield,
    MultiShoot,
    Count,
    Invalid = -1
}

public class PowerUpAuthoring : MonoBehaviour
{
    public float duration = 10.0f;
    public PowerUpType type = PowerUpType.Invalid;

    public class Baker : Baker<PowerUpAuthoring>
    {
        public override void Bake(PowerUpAuthoring authoring)
        {
            var data = new PowerUpData
            {
                duration = authoring.duration,
                type = authoring.type,
                picked = false
            };
            AddComponent(data);
        }
    }
}

struct PowerUpData : IComponentData
{
    public float duration;
    public bool picked;
    public PowerUpType type;
}
