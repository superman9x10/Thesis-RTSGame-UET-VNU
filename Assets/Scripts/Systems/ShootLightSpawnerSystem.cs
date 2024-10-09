using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach (RefRO<ShootAttack> shootAtk in SystemAPI.Query<RefRO<ShootAttack>>())
        {
            if (shootAtk.ValueRO.onShoot.isTrigger)
            {
                Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.shootLightPrefabEntity);
                SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootAtk.ValueRO.onShoot.shootFromPos));
            }
        }
    }
}
