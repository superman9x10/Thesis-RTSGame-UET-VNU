using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SetupUnitMoverDefaultPositionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<UnitMover> unitMover, RefRO<SetupUnitMoverDefaultPosition> setupUnitMoverDefaultPos, Entity entity)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<UnitMover>, RefRO<SetupUnitMoverDefaultPosition>>().WithEntityAccess())
        {
            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

            ecb.RemoveComponent<SetupUnitMoverDefaultPosition>(entity);
        }
    }
}
