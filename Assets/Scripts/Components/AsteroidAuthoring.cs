using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class AsteroidAuthoring : MonoBehaviour
{
    public class Baker : Baker<AsteroidAuthoring>
    {
        public override void Bake(AsteroidAuthoring authoring)
        {
            var data = new AsteroidData
            {
                size = 8,
                destroyed = false
            };
            AddComponent(data);
        }
    }
}

struct AsteroidData : IComponentData
{
    public bool destroyed;
    public int size;
}
