using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct ActiveAnimationSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataHolder>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

        foreach ((RefRW<ActiveAnimation> activeAnimation, RefRW<MaterialMeshInfo> materialMeshInfo) in
            SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
        {

            if (!activeAnimation.ValueRO.animationDataBlobAssetRef.IsCreated) 
            {
                activeAnimation.ValueRW.animationDataBlobAssetRef = animationDataHolder.soldierIdle;
            }

            activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
            if (activeAnimation.ValueRO.frameTimer > activeAnimation.ValueRO.animationDataBlobAssetRef.Value.frameTimerMax)
            {
                activeAnimation.ValueRW.frameTimer -= activeAnimation.ValueRO.animationDataBlobAssetRef.Value.frameTimerMax;
                activeAnimation.ValueRW.frame = (activeAnimation.ValueRW.frame + 1) % activeAnimation.ValueRO.animationDataBlobAssetRef.Value.frameMax;

                materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.animationDataBlobAssetRef.Value.batchMeshIdBlobArray[activeAnimation.ValueRO.frame];
            }
        }
    }
}
