using DG.Tweening;
using UnityEngine;

public class SoldierMono : BaseUnit
{
    [Header("Soldier")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject shootLightPrefab;
    [SerializeField] private GameObject selected;

    public bool CanMove { set; get; } = false;

    private float timeToAtk;
    private bool isAtk;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        UnSelected();
    }

    private void Update()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, atkRange, targetLayer);
        
        if (targets.Length != 0)
        {
            if (targetToAtk == null)
            {
                isAtk = false;
                targetToAtk = FindNearestTarget(targets);
            }
            else
            {
                isAtk = true;

                Vector3 targetPos = targetToAtk.GetComponent<ZombieMono>().hitPoint.position;
                Vector3 dir = (targetPos - transform.position).normalized;
                transform.rotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(new Vector3(dir.x, transform.position.y, dir.z), Vector3.up),
                    Time.deltaTime * rotateSpeed * 10f);

                if (timeToAtk <= 0)
                {

                    Shooting(targetPos);
                    timeToAtk = 1 / atkRate;

                }
                else
                {
                    timeToAtk -= Time.deltaTime;
                }
            }
        }
        else
        {
            isAtk = false;
            targetToAtk = null;
            timeToAtk = 1 / atkRate;
        }
    }

    private void FixedUpdate()
    {
        if (!CanMove) return;

        Vector3 moveDir = (moveTarget - transform.position);
        float reachedTargetDistanceSq = 2f;

        if (moveDir.magnitude <= reachedTargetDistanceSq)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            CanMove = false;
            return;
        }

        moveDir = moveDir.normalized;
        
        if(!isAtk)
        transform.rotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(moveDir, Vector3.up), Time.fixedDeltaTime * rotateSpeed);

        rb.linearVelocity = moveDir * moveSpeed;
        rb.angularVelocity = Vector3.zero;
    }

    public void SetMoveTarget(Vector3 _targetPos)
    {
        moveTarget = _targetPos;
        CanMove = true;
    }

    public void Shooting(Vector3 targetPos)
    {
        //Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = firePoint.position;
        bullet.transform.GetChild(0).GetComponent<BulletMono>().SetDir(targetPos);
        bullet.transform.GetChild(0).GetComponent<BulletMono>().damage = damage;
        //Spawn shootlight
        GameObject shootLight = Instantiate(shootLightPrefab);
        shootLight.transform.position = firePoint.position;
    }

    private GameObject FindNearestTarget(Collider[] targets)
    {
        GameObject nearestTarget = targets[0].gameObject;
        float minDist = Vector3.Distance(transform.position, nearestTarget.transform.position); ;

        for(int i = 1; i < targets.Length; i++) 
        {
            float dist = Vector3.Distance(targets[i].transform.position, transform.position);
            
            if (dist < minDist)
            {
                minDist = dist;
                nearestTarget = targets[i].gameObject;
            }
        }

        return nearestTarget;
    }

    public void Selected()
    {
        selected.SetActive(true);
    }

    public void UnSelected()
    {
        selected.SetActive(false);
    }

    public override void OnHit(int damage)
    {
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            Destroy(gameObject);
        }

        healthBar.UpdateHealthBar(curHP, maxHP);
    }
}
