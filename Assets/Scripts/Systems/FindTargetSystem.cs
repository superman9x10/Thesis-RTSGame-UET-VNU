using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

partial struct FindTargetSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<FindTarget> findTarget, RefRW<Target> target) 
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>, RefRW<Target>>())
        {
            findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            
            if (findTarget.ValueRW.timer > 0)
            {

                continue;
            }
            findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

            distanceHitList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNIT_LAYER,
                GroupIndex = 0
            };

            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity)) continue;

                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                    if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        // Valid target
                        target.ValueRW.targetEntity = distanceHit.Entity;
                        break;
                    }
                }
            }
        }
    }
}
