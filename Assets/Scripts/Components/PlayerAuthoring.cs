using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerAuthoring : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float accelerationSpeed = 10.0f;
    public float bulletSpeed = 15.0f;
    public float bulletLifeTime = 10.0f;
    public GameObject bulletPrefab;

    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var data = new PlayerData
            {
                accelerationSpeed = authoring.accelerationSpeed,
                rotationSpeed = authoring.rotationSpeed,
                bulletSpeed = authoring.bulletSpeed,
                bulletLifeTime = authoring.bulletLifeTime,
                bulletPrefab = GetEntity(authoring.bulletPrefab),
                invulnerabilityExpireTick = 0,
                multiShootExpireTick = 0,
                dead = false
            };
            AddComponent(data);
        }
    }
}

struct PlayerData : IComponentData
{
    public float accelerationSpeed;
    public float rotationSpeed;
    public float bulletSpeed;
    public float bulletLifeTime;
    public Entity bulletPrefab;
    public double invulnerabilityExpireTick;
    public double multiShootExpireTick;
    public bool dead;
}
