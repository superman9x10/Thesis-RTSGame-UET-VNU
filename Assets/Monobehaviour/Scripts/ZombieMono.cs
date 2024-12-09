using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMono : BaseUnit
{
    [Header("Zombie")]
    [SerializeField] private float timeToLostTarget;
    [SerializeField] private NavMeshAgent navMeshAgent;

    public Transform hqPos;
    private float timeToAtk;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        MoveToHQ();
    }
    private void FixedUpdate()
    {
        if (targetToAtk != null)
        {
            MoveToTarget();

            if (Vector3.Distance(transform.position, targetToAtk.transform.position) > 5f)
            {
                LostTargetAfter();
            }
            else
            {
                timeToLostTarget = 1f;
            }
        }
        else
        {
            FindTarget();
        }
    }

    private void MoveToHQ()
    {
        navMeshAgent.SetDestination(hqPos.position);
    }
    private void MoveToTarget()
    {
        //Vector3 moveDir = (targetToAtk.transform.position - transform.position).normalized;
        //transform.rotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(moveDir, Vector3.up), Time.fixedDeltaTime * rotateSpeed);

        //rb.linearVelocity = moveDir * moveSpeed;
        //rb.angularVelocity = Vector3.zero;

        navMeshAgent.SetDestination(targetToAtk.transform.position);
    }

    private void LostTargetAfter()
    {
        if (timeToLostTarget <= 0)
        {
            targetToAtk = null;
            timeToLostTarget = 1f;
        }
        else
        {
            timeToLostTarget -= Time.deltaTime;
        }
    }
    private void FindTarget()
    {
        var targets = Physics.OverlapSphere(transform.position, findTargetRange, targetLayer);
        
        if (targets != null && targets.Length > 0)
        {
            GameObject nearistTarget = FindNearestTarget(targets);
            targetToAtk = nearistTarget;
        }
    }

    private GameObject FindNearestTarget(Collider[] targets)
    {
        GameObject nearestTarget = targets[0].gameObject;
        float minDist = Vector3.Distance(transform.position, nearestTarget.transform.position); ;

        for (int i = 1; i < targets.Length; i++)
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

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Selectable"))
        {
            if (timeToAtk <= 0)
            {
                collision.gameObject.GetComponent<SoldierMono>().OnHit(damage);
                timeToAtk = 1 / atkRate;
            }
            else
            {
                timeToAtk -= Time.deltaTime;
            }
        }
    }
}
