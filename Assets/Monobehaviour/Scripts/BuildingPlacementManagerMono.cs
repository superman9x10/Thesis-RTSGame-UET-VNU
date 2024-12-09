using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManagerMono : MonoBehaviour
{
    public static BuildingPlacementManagerMono Instance { get; private set; }


    public event EventHandler OnActiveBuildingTypeSOChanged;


    [SerializeField] private BuildingTypeSO buildingTypeSO;
    [SerializeField] private UnityEngine.Material ghostMaterial;


    private Transform ghostTransform;


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (ghostTransform != null)
        {
            ghostTransform.position = MouseWorldPosition.Instance.GetPosition();
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (buildingTypeSO.IsNone())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetActiveBuildingTypeSO(GameAssets.Instance.buildingTypeListSO.none);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (ResourceManager.Instance.CanSpendResourceAmount(buildingTypeSO.buildCostResourceAmountArray))
            {
                ResourceManager.Instance.SpendResourceAmount(buildingTypeSO.buildCostResourceAmountArray);
                Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
                GameObject buildingConstructionVisualEntity = Instantiate(buildingTypeSO.GetVisualPrefabEntity());
                buildingConstructionVisualEntity.transform.position = mouseWorldPosition;
            }
        }
    }

    private bool CanPlaceBuilding()
    {
        

        return true;
    }


    public BuildingTypeSO GetActiveBuildingTypeSO()
    {
        return buildingTypeSO;
    }

    public void SetActiveBuildingTypeSO(BuildingTypeSO buildingTypeSO)
    {
        this.buildingTypeSO = buildingTypeSO;

        if (ghostTransform != null)
        {
            Destroy(ghostTransform.gameObject);
        }

        if (!buildingTypeSO.IsNone())
        {
            ghostTransform = Instantiate(buildingTypeSO.visualPrefab);
            foreach (MeshRenderer meshRenderer in ghostTransform.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }

        OnActiveBuildingTypeSOChanged?.Invoke(this, EventArgs.Empty);
    }
}
