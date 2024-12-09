using UnityEngine;

public class MonoHealthBar : MonoBehaviour
{
    public Transform bar;
    private Camera targetCamera;
    private void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                        targetCamera.transform.rotation * Vector3.up);
    }

    public void UpdateHealthBar(int curHP, int maxHP)
    {
        bar.localScale = new Vector3((float) curHP / maxHP, bar.localScale.y, bar.localScale.z); 
    }
}
