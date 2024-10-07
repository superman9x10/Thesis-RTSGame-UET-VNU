using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRO<LocalTransform> localTransform, RefRO<MoveOverride> moveOverride, RefRW<UnitMover> unitMover, EnabledRefRW<MoveOverride> moveOverrideEnabled)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveOverride>, RefRW<UnitMover>, EnabledRefRW<MoveOverride>>())
        {
            if (math.distancesq(localTransform.ValueRO.Position, moveOverride.ValueRO.targetPos) > UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ)
            {

                unitMover.ValueRW.targetPosition = moveOverride.ValueRO.targetPos;
            }
            else
            {
                moveOverrideEnabled.ValueRW = false;
            }
        }
    }
}
