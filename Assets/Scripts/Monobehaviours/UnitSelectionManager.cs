using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitSelesctionManager : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < unitMoverArray.Length; i++) 
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMoverArray[i] = unitMover;
                //entityManager.SetComponentData(entityArray[i], unitMover);
            }

            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }
}
