using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SphereColliderAuthoring : MonoBehaviour
{
    public float radius = 1.0f;
    public class Baker : Baker<SphereColliderAuthoring>
    {
        public override void Bake(SphereColliderAuthoring authoring)
        {
            var data = new SphereColliderData
            {
                radius = authoring.radius
            };
            AddComponent(data);
        }
    }
}

struct SphereColliderData : IComponentData
{
    public float radius;
}
