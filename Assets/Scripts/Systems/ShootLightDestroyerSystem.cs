using Unity.Burst;
using Unity.Entities;

partial struct ShootLightDestroyerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<ShootLight> shootLight, Entity entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
        {
            shootLight.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (shootLight.ValueRO.timer <= 0)
            {
                EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

                ecb.DestroyEntity(entity);
            }
        }
    }
}
