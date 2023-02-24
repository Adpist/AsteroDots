using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyAuthoring : MonoBehaviour
{
    public int score = 10;

    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var data = new EnemyData
            {
                score = authoring.score,
                destroyed = false
            };
            AddComponent(data);
        }
    }
}

struct EnemyData : IComponentData
{
    public bool destroyed;
    public int score;
}
