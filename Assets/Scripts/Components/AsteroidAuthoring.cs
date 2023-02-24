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
                size = 8
            };
            AddComponent(data);
        }
    }
}

struct AsteroidData : IComponentData
{
    public int size;
}
