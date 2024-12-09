using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManagerUIMono : MonoBehaviour
{
    [SerializeField] private RectTransform buildingContainer;
    [SerializeField] private RectTransform buildingTemplate;
    [SerializeField] private BuildingTypeListSO buildingTypeListSO;


    private Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle_Mono> buildingButtonDictionary;


    private void Awake()
    {
        buildingTemplate.gameObject.SetActive(false);

        buildingButtonDictionary = new Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle_Mono>();

        foreach (BuildingTypeSO buildingTypeSO in buildingTypeListSO.buildingTypeSOList)
        {
            if (!buildingTypeSO.showInBuildingPlacementManagerUI)
            {
                continue;
            }

            RectTransform buildingRectTransfrom = Instantiate(buildingTemplate, buildingContainer);
            buildingRectTransfrom.gameObject.SetActive(true);

            BuildingPlacementManagerUI_ButtonSingle_Mono buttonSingle =
                buildingRectTransfrom.GetComponent<BuildingPlacementManagerUI_ButtonSingle_Mono>();

            buildingButtonDictionary[buildingTypeSO] = buttonSingle;

            buttonSingle.Setup(buildingTypeSO);
        }
    }

    private void Start()
    {
        BuildingPlacementManagerMono.Instance.OnActiveBuildingTypeSOChanged += BuildingPlacementManager_OnActiveBuildingTypeSOChanged;

        UpdateSelectedVisual();
    }

    private void BuildingPlacementManager_OnActiveBuildingTypeSOChanged(object sender, System.EventArgs e)
    {
        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {
        foreach (BuildingTypeSO buildingTypeSO in buildingButtonDictionary.Keys)
        {
            buildingButtonDictionary[buildingTypeSO].HideSelected();
        }

        buildingButtonDictionary[BuildingPlacementManagerMono.Instance.GetActiveBuildingTypeSO()].
            ShowSelected();
    }
}
