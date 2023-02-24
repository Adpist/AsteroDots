using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BulletAuthoring : MonoBehaviour
{
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var data = new BulletStats
            {
                lifeTime = 100
            };
            AddComponent(data);
        }
    }
}

struct BulletStats : IComponentData
{
    public float lifeTime;
}