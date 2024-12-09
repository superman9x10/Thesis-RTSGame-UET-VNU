using UnityEngine;

public class GamePrefabManager : MonoBehaviour
{
    public static GamePrefabManager Instance;

    public GameObject buildingBarrackPrefab;
    public GameObject goldHarvesterPrefab;
    public GameObject ironHarvesterPrefab;
    public GameObject oilHarvesterPrefab;
    public GameObject townerPrefab;

    private void Awake()
    {
        Instance = this;
    }
}
