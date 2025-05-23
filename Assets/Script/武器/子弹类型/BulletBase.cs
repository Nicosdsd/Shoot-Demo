using System;
using UnityEngine;

// 子弹控制
public class BulletBase : MonoBehaviour
{
    public float damage = 1;
    public float destroyAfter = 5f;
    public float knockbackForce = 10f;
    public GameObject hitEffectPrefab; // 命中粒子效果的预制体
    private Weapon defaultWeapon;
    
    PlayerControl player; 
    void Start()
    {
        //AudioManager.Instance.PlaySound("普通子弹");
        Destroy(gameObject, destroyAfter);
        player = FindObjectOfType<PlayerControl>();
       

        defaultWeapon = player.currentWeapon;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果子弹击中了敌人
        if (other.CompareTag("Enemy"))
        {
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();

            if (enemyRigidbody != null && player!=null)
            {
                Vector3 knockbackDirection = (transform.position - player.transform.position).normalized;
                knockbackDirection.y = 0;

                // 应用水平击退方向的力
                enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                
                // 减少敌人生命值
                EnemyControl enemyControl = other.GetComponent<EnemyControl>();
                if (enemyControl != null)
                {
                    enemyControl.Hit(damage);
                }

                // 子弹溅射效果
                if (hitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    if (knockbackDirection != Vector3.zero) // 确保击退方向不为零
                    {
                        hitEffect.transform.rotation = Quaternion.LookRotation(-knockbackDirection); // 朝向击退的反方向
                    }

                    Destroy(hitEffect, 0.5f);
                }
                
                // 销毁子弹
                Destroy(gameObject);
            }
        }
    }
    

}