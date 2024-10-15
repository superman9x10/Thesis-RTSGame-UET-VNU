using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    private ComponentLookup<LocalTransform> _localTransformComponentLookup;
    private ComponentLookup<Health> _healthComponentLockup;
    private ComponentLookup<PostTransformMatrix> _postTransformMatrixComponentLookup;

    public void OnCreate(ref SystemState state)
    {
        _localTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        _healthComponentLockup = state.GetComponentLookup<Health>(true);
        _postTransformMatrixComponentLookup = state.GetComponentLookup<PostTransformMatrix>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        
        if(Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        #region With Job

        _localTransformComponentLookup.Update(ref state);
        _healthComponentLockup.Update(ref state);
        _postTransformMatrixComponentLookup.Update(ref state);

        HealthBarJob healthBarJob = new HealthBarJob
        {
            localTransformComponentLookup = _localTransformComponentLookup,
            healthComponentLockup = _healthComponentLockup,
            postTransformMatrixComponentLookup = _postTransformMatrixComponentLookup,
            cameraForward = cameraForward
        };

        healthBarJob.ScheduleParallel();

        #endregion

        #region Without Job

        //foreach ((RefRW<LocalTransform> localTransform, RefRO<HealthBar> healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>())
        //{
        //    LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);

        //    if (localTransform.ValueRO.Scale == 1f)
        //    {
        //        localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        //    }

        //    Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

        //    if (!health.onHealthChanged)
        //    {
        //        continue;
        //    }

        //    float healthNormalize = (float)health.healthAmount / health.healthAmountMax;

        //    if (healthNormalize < 1f)
        //    {
        //        localTransform.ValueRW.Scale = 1f;
        //    }
        //    else
        //    {
        //        localTransform.ValueRW.Scale = 0f;
        //    }

        //    RefRW<PostTransformMatrix> barVisualPostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
        //    barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalize, 1, 1);
        //}

        #endregion
    }
}

[BurstCompile]
public partial struct HealthBarJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<Health> healthComponentLockup;

    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    public float3 cameraForward;

    public void Execute(in HealthBar healthBar, Entity entity)
    {
        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform parentLocalTransform = localTransformComponentLookup[healthBar.healthEntity];

        if (localTransform.ValueRW.Scale == 1f)
        {
            localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        }

        Health health = healthComponentLockup[healthBar.healthEntity];

        if (!health.onHealthChanged)
        {
            return;
        }

        float healthNormalize = (float)health.healthAmount / health.healthAmountMax;

        if (healthNormalize < 1f)
        {
            localTransform.ValueRW.Scale = 1f;
        }
        else
        {
            localTransform.ValueRW.Scale = 0f;
        }

        RefRW<PostTransformMatrix> barVisualPostTransformMatrix = postTransformMatrixComponentLookup.GetRefRW(healthBar.barVisualEntity);
        barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalize, 1, 1);
    }
}
