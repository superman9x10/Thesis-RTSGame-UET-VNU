using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ActiveAnimationAuthoring : MonoBehaviour
{
    public Mesh frame0;
    public Mesh frame1;

    public class Baker : Baker<ActiveAnimationAuthoring>
    {
        public override void Bake(ActiveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            AddComponent(entity, new ActiveAnimation
            {
                frame0 = entitiesGraphicSystem.RegisterMesh(authoring.frame0),
                frame1 = entitiesGraphicSystem.RegisterMesh(authoring.frame1),      
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
