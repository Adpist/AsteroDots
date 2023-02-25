using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum EnemyType
{
    Asteroid,
    UFO,
    Count,
    Invalid
}

public class EnemyAuthoring : MonoBehaviour
{
    public int score = 10;
    public EnemyType enemyType = EnemyType.Invalid;

    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var data = new EnemyData
            {
                score = authoring.score,
                enemyType = authoring.enemyType
            };
            AddComponent(data);
        }
    }
}

struct EnemyData : IComponentData
{
    public EnemyType enemyType;
    public int score;
}
