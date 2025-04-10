using System;
using UnityEngine;

public class EnemyShootTrigger : MonoBehaviour
{
    public EnemyShoot EnemyShoot;
    Coroutine shootingCoroutine;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 确保不会启动多个协程
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(EnemyShoot.ShootingRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 停止协程并清空引用
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }

            // 停止射击状态
            EnemyShoot.canShoot = false;
        }
    }
}
