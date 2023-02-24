using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerStatsAuthoring : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float accelerationSpeed = 10.0f;
    public float bulletSpeed = 15.0f;
    public float bulletLifeTime = 10.0f;
    public GameObject bulletPrefab;

    public class Baker : Baker<PlayerStatsAuthoring>
    {
        public override void Bake(PlayerStatsAuthoring authoring)
        {
            var data = new PlayerStats
            {
                accelerationSpeed = authoring.accelerationSpeed,
                rotationSpeed = authoring.rotationSpeed,
                bulletSpeed = authoring.bulletSpeed,
                bulletLifeTime = authoring.bulletLifeTime,
                bulletPrefab = GetEntity(authoring.bulletPrefab),
                dead = false
            };
            AddComponent(data);
        }
    }
}

struct PlayerStats : IComponentData
{
    public float accelerationSpeed;
    public float rotationSpeed;
    public float bulletSpeed;
    public float bulletLifeTime;
    public Entity bulletPrefab;
    public bool dead;
}
