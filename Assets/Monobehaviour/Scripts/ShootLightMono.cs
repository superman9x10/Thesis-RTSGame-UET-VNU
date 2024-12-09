using UnityEngine;

public class ShootLightMono : MonoBehaviour
{
    [SerializeField] private float timer;

    private void Update()
    {
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
