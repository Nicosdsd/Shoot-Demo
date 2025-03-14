using System;
using System.Collections;
using UnityEngine;

public class TurretControl : MonoBehaviour
{
    private Transform currentTarget; // 自动瞄准的当前目标
    private PlayerControl player; // 主角的位置引用
    public Vector3[] offsets;
    public Vector3 offset ; // 炮台相对于主角的位置偏移
    public float autoAimRadius = 20f; // 锁定范围
    float minAimAngleThreshold = 5f; // 瞄准允许射击角度
    private Vector3 fireDirection; // 瞄准方向
    public float rotationSpeed = 10f; // 旋转速度
    public Weapon currentWeapon; // 当前装备武器
    public Transform firePoint;
    private float lastShotTime;
    private int addWeaponNum;

    private void Start()
    {
        player = FindObjectOfType<PlayerControl>();
        Getoffset();
    }

    private void Getoffset()
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("AddWeapon");
        int addWeaponNum = objectsWithTag.Length;
        if (addWeaponNum > 0 && addWeaponNum <= offsets.Length)
        {
            offset = offsets[addWeaponNum - 1];
        }
    }

    void Update()
    {
        if (player != null)
        {
            // 更新炮台位置以跟随主角
            FollowPlayer();
        }

        // 索敌
        currentTarget = FindClosestTarget(autoAimRadius);
    }

    void FixedUpdate()
    {
        // 在物理更新中进行缓慢旋转
        if (currentTarget != null)
        {
            FireAim();
        }
    }

    private void FollowPlayer()
    {
        transform.position = player.transform.position + offset;
    }

    // 瞄准并开火
    private void FireAim()
    {
        if (currentTarget == null || currentWeapon == null) return;

        // 计算目标方向，并忽略Y轴上的变化
        Vector3 adjustedDirection = currentTarget.position - currentWeapon.bulletSpawnPoint.position;
       // adjustedDirection.y = 0; // 忽略Y轴的旋转

        // 归一化方向向量
        fireDirection = adjustedDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (angleDifference < minAimAngleThreshold)
        {
            currentWeapon.Fire(fireDirection);
        }
    }

    private Transform FindClosestTarget(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Vector3 directionToTarget = hitCollider.transform.position - transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestTarget = hitCollider.transform;
                }
            }
        }

        return closestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawSphere(transform.position, autoAimRadius);
    }
}