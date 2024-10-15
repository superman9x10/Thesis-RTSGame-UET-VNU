using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using static UnityEngine.GraphicsBuffer;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    private ComponentLookup<LocalTransform> _localTransformComponentLookup;
    private EntityStorageInfoLookup _entityStorageInfoLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
        _entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        #region With Job

        _localTransformComponentLookup.Update(ref state);
        _entityStorageInfoLookup.Update(ref state);

        ResetTargetJob resetTargetJobn = new ResetTargetJob
        {
            localTransformComponentLookup = _localTransformComponentLookup,
            entityStorageInfoLookup = _entityStorageInfoLookup
        };

        resetTargetJobn.ScheduleParallel();


        ResetTargetOverrideJob resetTargetOverrideJob = new ResetTargetOverrideJob
        {
            localTransformComponentLookup = _localTransformComponentLookup,
            entityStorageInfoLookup = _entityStorageInfoLookup
        };

        resetTargetOverrideJob.ScheduleParallel();
        #endregion


        #region Without Job

        //foreach (RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        //{
        //    if(target.ValueRW.targetEntity != Entity.Null)
        //    {
        //        if (!SystemAPI.Exists(target.ValueRW.targetEntity) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity))
        //        {
        //            target.ValueRW.targetEntity = Entity.Null;
        //        }
        //    }
        //}

        //foreach (RefRW<TargetOverride> targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
        //{
        //    if (targetOverride.ValueRW.targetEntity != Entity.Null)
        //    {
        //        if (!SystemAPI.Exists(targetOverride.ValueRW.targetEntity) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.targetEntity))
        //        {
        //            targetOverride.ValueRW.targetEntity = Entity.Null;
        //        }
        //    }
        //}

        #endregion

    }
}

[BurstCompile]
public partial struct ResetTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref Target target)
    {
        if (target.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(target.targetEntity) || !localTransformComponentLookup.HasComponent(target.targetEntity))
            {
                target.targetEntity = Entity.Null;
            }
        }
    }
}

[BurstCompile]
public partial struct ResetTargetOverrideJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;

    public void Execute(ref TargetOverride targetOverride)
    {
        if (targetOverride.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(targetOverride.targetEntity) || !localTransformComponentLookup.HasComponent(targetOverride.targetEntity))
            {
                targetOverride.targetEntity = Entity.Null;
            }
        }
    }
}
