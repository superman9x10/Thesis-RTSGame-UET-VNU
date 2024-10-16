using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
partial struct BulletMoverSystem : ISystem
{
    private ComponentLookup<Health> _healthComponentLookup;
    private ComponentLookup<ShootVictim> _shootVictimComponentLookup;
    private ComponentLookup<LocalTransform> _localTransformComponentLookup;

    private EntityCommandBuffer ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _healthComponentLookup = state.GetComponentLookup<Health>();
        _shootVictimComponentLookup = state.GetComponentLookup<ShootVictim>(true);
        _localTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        #region With Job

        _healthComponentLookup.Update(ref state);
        _shootVictimComponentLookup.Update(ref state);
        _localTransformComponentLookup.Update(ref state);

        BulletMoverJob bulletMoveJob = new BulletMoverJob
        {
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
            healthComponentLookup = _healthComponentLookup,
            localTransformComponentLookup = _localTransformComponentLookup,
            shootVictimComponentLookup = _shootVictimComponentLookup
        };

        bulletMoveJob.ScheduleParallel();
        #endregion

        #region Without Job

        //foreach ((RefRW<LocalTransform> localTransform, RefRO<Bullet> bullet, RefRO<Target> target, Entity entity) 
        //    in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
        //{
        //if (target.ValueRO.targetEntity == Entity.Null)
        //{
        //    ecb.DestroyEntity(entity);
        //    continue;
        //}

        //LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
        //ShootVictim targetShootVictiom = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
        //float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictiom.hitLocalPosition);

        //float distanceBefor = math.distancesq(localTransform.ValueRO.Position, targetPosition);

        //float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
        //moveDirection = math.normalize(moveDirection);
        //localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

        //float distanceAfter = math.distancesq(localTransform.ValueRO.Position, targetPosition);

        //if(distanceAfter > distanceBefor)
        //{
        //    // Overshot
        //    localTransform.ValueRW.Position = targetPosition;
        //}

        //float destroyDistanceSq = 0.2f;

        //if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
        //{
        //    RefRW<Health> targethealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
        //    targethealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
        //    targethealth.ValueRW.onHealthChanged = true;

        //    ecb.DestroyEntity(entity);
        //}
        //}

        #endregion
    }
}

[BurstCompile]
public partial struct BulletMoverJob : IJobEntity
{
    public float deltaTime;

    [NativeDisableParallelForRestriction] public EntityCommandBuffer ecb;
    [ReadOnly] public ComponentLookup<ShootVictim> shootVictimComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<Health> healthComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;

    public void Execute(in Bullet bullet, in Target target, Entity entity)
    {
        if (target.targetEntity == Entity.Null)
        {
            ecb.DestroyEntity(entity);
            return;
        }

        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform targetLocalTransform = localTransformComponentLookup[target.targetEntity];
        ShootVictim targetShootVictiom = shootVictimComponentLookup[target.targetEntity];

        float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictiom.hitLocalPosition);

        float distanceBefor = math.distancesq(localTransform.ValueRO.Position, targetPosition);

        float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
        moveDirection = math.normalize(moveDirection);
        localTransform.ValueRW.Position += moveDirection * bullet.speed * deltaTime;

        float distanceAfter = math.distancesq(localTransform.ValueRO.Position, targetPosition);

        if (distanceAfter > distanceBefor)
        {
            // Overshot
            localTransform.ValueRW.Position = targetPosition;
        }

        float destroyDistanceSq = 0.2f;

        if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
        {
            RefRW<Health> targethealth = healthComponentLookup.GetRefRW(target.targetEntity);
            targethealth.ValueRW.healthAmount -= bullet.damageAmount;
            targethealth.ValueRW.onHealthChanged = true;

            ecb.DestroyEntity(entity);
        }
    }
}
