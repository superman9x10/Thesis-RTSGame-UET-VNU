using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthDeadTestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRO<Health> health, Entity entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
        {
            if(health.ValueRO.healthAmount <= 0)
            {
                //This entity is dead
                ecb.DestroyEntity(entity);
            }
        }
    }

}