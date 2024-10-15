using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        #region With Job

        new ResetShootAtkJob().ScheduleParallel();
        new ResetHealthJob().ScheduleParallel();
        new ResetSelectedJob().ScheduleParallel();

        #endregion


        #region Without Job

        //foreach (RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        //{
        //    selected.ValueRW.onDeselected = false;
        //    selected.ValueRW.onSelected = false;
        //}

        //foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>())
        //{
        //    health.ValueRW.onHealthChanged = false;
        //}

        //foreach (RefRW<ShootAttack> shootAtk in SystemAPI.Query<RefRW<ShootAttack>>())
        //{
        //    shootAtk.ValueRW.onShoot.isTrigger = false;
        //}

        #endregion
    }
}

[BurstCompile]
public partial struct ResetShootAtkJob : IJobEntity
{
    public void Execute(ref ShootAttack shootAtk)
    {
        shootAtk.onShoot.isTrigger = false;
    }
}

[BurstCompile]
public partial struct ResetHealthJob : IJobEntity
{
    public void Execute(ref Health health)
    {
        health.onHealthChanged = false;
    }
}

[BurstCompile]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ResetSelectedJob : IJobEntity
{
    public void Execute(ref Selected selected)
    {
        selected.onDeselected = false;
        selected.onSelected = false;
    }
}
