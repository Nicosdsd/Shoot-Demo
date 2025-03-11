using System.Collections;
using UnityEngine;


public class TurretControl : MonoBehaviour
{
    private Transform currentTarget; // 自动瞄准的当前目标
    public float autoAimRadius = 20f; // 锁定范围
    float minAimAngleThreshold = 5f; //瞄准允许射击角度
    private Vector3 fireDirection; // 瞄准方向
    public float rotationSpeed = 10f; // 旋转速度
    public TurretWeapon currentWeapon; // 当前装备武器（假设有一个Weapon类处理射击）
    private float lastShotTime; 
    
    

    void Update()
    {
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

    // 瞄准并开火
    private void FireAim()
    {
        // 计算目标方向，并忽略Y轴上的变化
        Vector3 adjustedDirection = currentTarget.position - currentWeapon.bulletSpawnPoint.position;
        adjustedDirection.y = 0; // 忽略Y轴的旋转

        // 归一化方向向量
        fireDirection = adjustedDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);
        
        // 计算当前角色旋转与目标方向之间的角度差
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // 平滑插值使角色逐渐旋转到目标方向
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed );
        
        // 当角度差小于设定的阈值，并且可以射击时，才允许开火
        if (angleDifference < minAimAngleThreshold )
        {
            //开火
            currentWeapon.Fire(fireDirection);
        }
    }

    // 索敌
    private Transform FindClosestTarget(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy")) // 确保只检查带有 "Enemy" 标签的对象
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
        Gizmos.color = new Color(0, 1, 0, 0.25f); // 设置Gizmos颜色为绿色，透明度为0.25
        // 在玩家的位置绘制球体表示自动锁定区域
        Gizmos.DrawSphere(transform.position, autoAimRadius);
    }
}