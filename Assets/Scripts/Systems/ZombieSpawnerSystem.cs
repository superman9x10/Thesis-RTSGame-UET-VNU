using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor.Search;

partial struct ZombieSpawnerSystem : ISystem
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

        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner) in 
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
        {
            zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (zombieSpawner.ValueRO.timer > 0) continue;

            zombieSpawner.ValueRW.timer = zombieSpawner.ValueRO.timerMax;
            Entity zombieEntity = state.EntityManager.Instantiate(entitiesReferences.zombiePrefabEntity);
            SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            ecb.AddComponent<RandomWalking>(zombieEntity, new RandomWalking
            {
                originPosition = localTransform.ValueRO.Position,
                targetPosition = localTransform.ValueRO.Position,
                distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
                distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
                random = new Unity.Mathematics.Random((uint)zombieEntity.Index)
            });
        }
    }
}
