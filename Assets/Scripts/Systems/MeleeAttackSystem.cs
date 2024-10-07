using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton =  SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<RaycastHit> rayCastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> LocalTransform, RefRW<MeleeAttack> meleeAttack, RefRO<Target> target, RefRW<UnitMover> unitMover) 
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>()) 
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            float maxDistSq = 2f;
            bool isCloseEnoughToAttack = math.distancesq(LocalTransform.ValueRO.Position, targetLocalTransform.Position) < maxDistSq;

            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack)
            {
                //Raycast
                float3 dirToTarget = targetLocalTransform.Position - LocalTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                float disExtraToTestRaycast = 0.4f;

                RaycastInput rayCastInput = new RaycastInput()
                {
                    Start = LocalTransform.ValueRO.Position,
                    End = LocalTransform.ValueRO.Position + dirToTarget * meleeAttack.ValueRO.colliderSize + disExtraToTestRaycast,
                    Filter = CollisionFilter.Default
                };

                rayCastHitList.Clear();
                if (collisionWorld.CastRay(rayCastInput, ref rayCastHitList))
                {
                    foreach (RaycastHit rayCastHit in rayCastHitList)
                    {
                        if (rayCastHit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                //Cant atk
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                //Can atk
                unitMover.ValueRW.targetPosition = LocalTransform.ValueRO.Position;
                
                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                
                if (meleeAttack.ValueRO.timer <= 0)
                {
                    meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                    RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);

                    targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                    targetHealth.ValueRW.onHealthChanged = true;
                }

            }
        }
    }
}
