using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MoveOverrrideAuthoring : MonoBehaviour
{
    public class Baker : Baker<MoveOverrrideAuthoring>
    {
        public override void Bake(MoveOverrrideAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveOverride());
            SetComponentEnabled<MoveOverride>(entity, false);
        }
    }
}

public struct MoveOverride : IComponentData, IEnableableComponent
{
    public float3 targetPos;
}
