using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<LocalTransform> localTransform, RefRO<Bullet> bullet, RefRO<Target> target, Entity entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            ShootVictim targetShootVictiom = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictiom.hitLocalPosition);

            float distanceBefor = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);
            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            float distanceAfter = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            if(distanceAfter > distanceBefor)
            {
                // Overshot
                localTransform.ValueRW.Position = targetPosition;
            }

            float destroyDistanceSq = 0.2f;

            if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
            {
                RefRW<Health> targethealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targethealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                targethealth.ValueRW.onHealthChanged = true;

                ecb.DestroyEntity(entity);
            }
        }
    }
}
