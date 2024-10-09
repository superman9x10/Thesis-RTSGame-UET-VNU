using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct LoseTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRO<LocalTransform> LocalTransform, RefRW<Target> target, RefRO<LoseTarget> loseTarget, RefRO<TargetOverride> targetOverride)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Target>, RefRO<LoseTarget>, RefRO<TargetOverride>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;
            if(targetOverride.ValueRO.targetEntity  != Entity.Null) { continue; }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            if (math.distance(LocalTransform.ValueRO.Position, targetLocalTransform.Position) > loseTarget.ValueRO.loseTargetDistance)
            {
                target.ValueRW.targetEntity = Entity.Null;
            }
        }
    }
}
