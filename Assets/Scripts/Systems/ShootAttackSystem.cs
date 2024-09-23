using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack, RefRO<Target> target, RefRW<UnitMover> unitMover) 
            in SystemAPI.Query<RefRW<LocalTransform> , RefRW<ShootAttack>, RefRO<Target>, RefRW<UnitMover>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            if (math.distance(targetLocalTransform.Position, localTransform.ValueRO.Position) > shootAttack.ValueRO.attackDistance)
            {
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
                continue;
            }
            else
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            }

            //Rotation
            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);
            quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if(shootAttack.ValueRW.timer > 0) continue;
            shootAttack.ValueRW.timer = shootAttack.ValueRW.timerMax;

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(/*localTransform.ValueRO.Position*/bulletSpawnWorldPosition));
            
            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;
        }
    }
}
