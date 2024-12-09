using UnityEngine;

public abstract class BaseUnit : MonoBehaviour
{
    [Header("Health")]
    public int curHP;
    public int maxHP;

    [Header("Attack")]
    public int damage;
    public float atkRate;
    public float atkRange;
    public float findTargetRange;

    [Header("Movement")]
    public float moveSpeed;
    public float rotateSpeed;

    [Header("Config")]
    public LayerMask targetLayer;
    public Transform hitPoint;
    public MonoHealthBar healthBar;

    protected Rigidbody rb;
    protected Vector3 moveTarget;
    protected GameObject targetToAtk;

    public abstract void OnHit(int damage);
}
