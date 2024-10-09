using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

partial struct FindTargetSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<FindTarget> findTarget, RefRW<Target> target, RefRO<TargetOverride> targetoverride) 
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>, RefRW<Target>, RefRO<TargetOverride>>())
        {
            findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            
            if (findTarget.ValueRW.timer > 0) continue;
            findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

            if (targetoverride.ValueRO.targetEntity != Entity.Null)
            {
                target.ValueRW.targetEntity = targetoverride.ValueRO.targetEntity;
                continue;
            }

            distanceHitList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNIT_LAYER,
                GroupIndex = 0
            };

            Entity closestTargetEntity = Entity.Null;
            float  closestTargetDistance = float.MaxValue;
            float curTargetDistanceOffset = 0f;

            if (target.ValueRO.targetEntity != Entity.Null)
            {
                closestTargetEntity = target.ValueRO.targetEntity;
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(closestTargetEntity);
                closestTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
                curTargetDistanceOffset = 2f;
            }

            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity)) continue;

                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                    if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        // Valid target
                        if (closestTargetEntity == Entity.Null)
                        {
                            closestTargetEntity = distanceHit.Entity;
                            closestTargetDistance = distanceHit.Distance;
                        }
                        else
                        {
                            if (distanceHit.Distance + curTargetDistanceOffset < closestTargetDistance)
                            {
                                closestTargetEntity = distanceHit.Entity;
                                closestTargetDistance = distanceHit.Distance;
                            }
                        }
                        
                    }
                }
            } 

            if (closestTargetEntity != Entity.Null)
            {
                target.ValueRW.targetEntity = closestTargetEntity;
            }
        }
    }
}
