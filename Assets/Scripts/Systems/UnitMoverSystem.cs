using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
partial struct UnitMoverSystem : ISystem
{
    //[BurstCompile] 
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (localTransform, moveSpeed, physicsVelocity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>, RefRW<PhysicsVelocity>>())
        {
            float3 targetPosition = MouseWorldPosition.Instance.GetPosition();
            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            float rotationSpeed = 10f;

            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, quaternion.LookRotation(moveDirection, math.up()), SystemAPI.Time.DeltaTime * rotationSpeed);
            physicsVelocity.ValueRW.Linear = moveDirection * moveSpeed.ValueRO.value;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }
    }
}
