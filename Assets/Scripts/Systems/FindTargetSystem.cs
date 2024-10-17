using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

partial struct FindTargetSystem : ISystem
{
    private ComponentLookup<LocalTransform> _localTransformComponentLookup;
    private ComponentLookup<Unit> _unitComponentLookup;
    private EntityStorageInfoLookup _entityStorageInfoLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
        _unitComponentLookup = state.GetComponentLookup<Unit>(true);
        _entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton _physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld _collisionWorld = _physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> _distanceHitList = new NativeList<DistanceHit>(Allocator.TempJob);

        #region With Job

        //_localTransformComponentLookup.Update(ref state);
        //_unitComponentLookup.Update(ref state);
        //_entityStorageInfoLookup.Update(ref state);

        //FindTargetJob findTargetJob = new FindTargetJob
        //{
        //    deltaTime = SystemAPI.Time.DeltaTime,
        //    unitLayer = GameAssets.UNIT_LAYER,
        //    physicsWorldSingleton = _physicsWorldSingleton,
        //    collisionWorld = _collisionWorld,
        //    distanceHitList = _distanceHitList,
        //    localTransformComponentLookup = _localTransformComponentLookup,
        //    unitComponentLookup = _unitComponentLookup,
        //    entityStorageInfoLookup = _entityStorageInfoLookup
        //};

        //findTargetJob.ScheduleParallel();

        #endregion

        #region Without Job

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

            _distanceHitList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNIT_LAYER,
                GroupIndex = 0
            };

            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            float curTargetDistanceOffset = 0f;

            if (target.ValueRO.targetEntity != Entity.Null)
            {
                closestTargetEntity = target.ValueRO.targetEntity;
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(closestTargetEntity);
                closestTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
                curTargetDistanceOffset = 2f;
            }

            if (_collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref _distanceHitList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in _distanceHitList)
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

        #endregion

    }
}

[BurstCompile]
public partial struct FindTargetJob : IJobEntity
{
    public float deltaTime;
    public int unitLayer;

    [ReadOnly] public PhysicsWorldSingleton physicsWorldSingleton;
    [ReadOnly] public CollisionWorld collisionWorld;
    [NativeDisableParallelForRestriction] public NativeList<DistanceHit> distanceHitList;

    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public ComponentLookup<Unit> unitComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;

    public void Execute(ref FindTarget findTarget, ref Target target, in TargetOverride targetOverride, Entity entity)
    {
        findTarget.timer -= deltaTime;

        if (findTarget.timer > 0) return;
        findTarget.timer = findTarget.timerMax;

        if (targetOverride.targetEntity != Entity.Null)
        {
            target.targetEntity = targetOverride.targetEntity;
            return;
        }

        distanceHitList.Clear();
        CollisionFilter collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1u << unitLayer,
            GroupIndex = 0
        };

        Entity closestTargetEntity = Entity.Null;
        float closestTargetDistance = float.MaxValue;
        float curTargetDistanceOffset = 0f;
        LocalTransform localTransform = localTransformComponentLookup[entity];

        if (target.targetEntity != Entity.Null)
        {
            closestTargetEntity = target.targetEntity;
            LocalTransform targetLocalTransform = localTransformComponentLookup[closestTargetEntity];
            closestTargetDistance = math.distance(localTransform.Position, targetLocalTransform.Position);
            curTargetDistanceOffset = 2f;
        }

        if (collisionWorld.OverlapSphere(localTransform.Position, findTarget.range, ref distanceHitList, collisionFilter))
        {
            foreach (DistanceHit distanceHit in distanceHitList)
            {
                if (!entityStorageInfoLookup.Exists(distanceHit.Entity) || !unitComponentLookup.HasComponent(distanceHit.Entity)) continue;

                Unit targetUnit = unitComponentLookup[distanceHit.Entity];
                if (targetUnit.faction == findTarget.targetFaction)
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
            target.targetEntity = closestTargetEntity;
        }
    }
}
