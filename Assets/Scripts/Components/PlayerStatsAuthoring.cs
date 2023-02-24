using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerStatsAuthoring : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float accelerationSpeed = 10.0f;

    public class Baker : Baker<PlayerStatsAuthoring>
    {
        public override void Bake(PlayerStatsAuthoring authoring)
        {
            var data = new PlayerStats
            {
                accelerationSpeed = authoring.accelerationSpeed,
                rotationSpeed = authoring.rotationSpeed
            };
            AddComponent(data);
        }
    }
}

struct PlayerStats : IComponentData
{
    public float accelerationSpeed;
    public float rotationSpeed;
}
