using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static int UNIT_LAYER = 6;

    public static GameAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
