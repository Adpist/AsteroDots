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
            LifeTimeData lifeTimeData = new LifeTimeData
            {
                expirationTick = double.MaxValue
            };

            AddComponent(new BulletTag { });
            AddComponent(new LifeTimeData { });
        }
    }
}

struct BulletTag : IComponentData { }