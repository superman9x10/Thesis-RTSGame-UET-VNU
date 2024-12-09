using TMPro;
using Unity.Transforms;
using UnityEngine;

public class BulletMono : MonoBehaviour
{
    [SerializeField] private GameObject holder;
    [SerializeField] private float speed;

    private Vector3 moveDir;
    private Vector3 targetPos;

    public int damage;
    private void Update()
    {
        float distanceBeforeSq = (targetPos - transform.position).magnitude;

        transform.position += moveDir * speed * Time.deltaTime;

        float distanceAfterSq = (targetPos - transform.position).magnitude;

        if (distanceAfterSq > distanceBeforeSq)
        {
            // Overshot
            transform.position = targetPos;
            Destroy(holder);
        }
    }

    public void SetDir(Vector3 _targetPos)
    {
        targetPos = _targetPos;
        moveDir = (_targetPos - transform.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<ZombieMono>().OnHit(damage);
        }
    }
}
