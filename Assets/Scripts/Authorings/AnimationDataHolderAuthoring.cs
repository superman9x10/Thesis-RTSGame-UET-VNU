using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
    public AnimationDataSo soldierIdle;

    public class Baker : Baker<AnimationDataHolderAuthoring>
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AnimationDataHolder animationDataHolder = new AnimationDataHolder();
            EntitiesGraphicsSystem entitiesGraphicSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);

            ref AnimationData animationData = ref blobBuilder.ConstructRoot<AnimationData>();
            animationData.frameTimerMax = authoring.soldierIdle.frameTimerMax;
            animationData.frameMax = authoring.soldierIdle.meshArray.Length;

            BlobBuilderArray<BatchMeshID> blobBuilderArray = blobBuilder.Allocate(ref animationData.batchMeshIdBlobArray, authoring.soldierIdle.meshArray.Length);

            for (int i = 0; i < authoring.soldierIdle.meshArray.Length; i++)
            {
                Mesh mesh = authoring.soldierIdle.meshArray[i];
                blobBuilderArray[i] = entitiesGraphicSystem.RegisterMesh(mesh);
            }

            animationDataHolder.soldierIdle = blobBuilder.CreateBlobAssetReference<AnimationData>(Allocator.Persistent);
            blobBuilder.Dispose();
            AddBlobAsset(ref animationDataHolder.soldierIdle, out Unity.Entities.Hash128 objectHash);

            AddComponent(entity, animationDataHolder);
        }
    }
}
public struct AnimationDataHolder : IComponentData
{
    public BlobAssetReference<AnimationData> soldierIdle;
}
public struct AnimationData
{
    public float frameTimerMax;
    public int frameMax;
    public BlobArray<BatchMeshID> batchMeshIdBlobArray;
}