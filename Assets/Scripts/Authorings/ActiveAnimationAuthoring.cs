using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ActiveAnimationAuthoring : MonoBehaviour
{
    public AnimationDataSo soldier_idle;

    public class Baker : Baker<ActiveAnimationAuthoring>
    {
        public override void Bake(ActiveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            AddComponent(entity, new ActiveAnimation
            {
                frame0 = entitiesGraphicSystem.RegisterMesh(authoring.soldier_idle.meshArray[0]),
                frame1 = entitiesGraphicSystem.RegisterMesh(authoring.soldier_idle.meshArray[1]),      
                maxFrame = authoring.soldier_idle.meshArray.Length,
                frameTimerMax = authoring.soldier_idle.frameTimerMax,
            });
        }
    }
}

public struct ActiveAnimation : IComponentData
{
    public int frame;
    public int maxFrame;
    public float frameTimer;
    public float frameTimerMax;

    public BatchMeshID frame0;
    public BatchMeshID frame1;
}
