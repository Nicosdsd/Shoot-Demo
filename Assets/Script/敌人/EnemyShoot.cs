using System;
using System.Collections;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    private PlayerControl player;//玩家控制器
    [Header("发射物")] 
    public bool canShoot;//进入攻击范围
    //public GameObject targetPrefab;      // 目标点
    public GameObject bulletPrefab;  // 子弹的预制体
    public Transform firePoint;//发射点
    public float bulletSpeed = 10;
    public float shootInterval = 2;

    
    private void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
    }

    public IEnumerator ShootingRoutine()
    {
        // 开始射击状态
        canShoot = true;

        while (canShoot)
        {
            Shooting(); // 执行射击逻辑
            yield return new WaitForSeconds(shootInterval); // 每隔 shootInterval 秒进行一次射击
        }
    }
    
    public void Shooting()
    {
        
        // 实例化子弹目标点
        Vector3 targetPos = player.transform.position;
        targetPos.y = firePoint.position.y;  // 确保子弹的发射高度与发射点一致

        // 实例化子弹
        GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();

        // 计算发射方向
        Vector3 direction = targetPos - firePoint.position;
        direction.y = 0;     // 只保留水平分量
        direction.Normalize(); // 归一化，确保方向向量的长度为1
        
        // 应用初速度
        bulletRigidbody.linearVelocity = direction * bulletSpeed;
    }
}
