using UnityEngine;

// 子弹控制
public class BulletController : MonoBehaviour
{
    public float destroyAfter = 5f;
    public float knockbackForce = 10f;
    public float upwardForce = 5f; // 控制新添加的向上力
    public GameObject hitEffectPrefab; // 命中粒子效果的预制体

    PlayerControl player; 
    void Start()
    {
        Destroy(gameObject, destroyAfter);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 如果子弹击中了敌人
        if (other.CompareTag("Enemy"))
        {
            // 向敌人施加击退力和向上的力
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();

            if (enemyRigidbody != null)
            {
                // 计算敌人的水平击退方向 （从子弹位置指向敌人位置）
                //Vector3 knockbackDirection = other.transform.position - transform.position;

                Vector3 knockbackDirection = (transform.position - player.transform.position).normalized;
                knockbackDirection.y = 0;


                // 应用水平击退方向的力
                enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // 应用向上的力
                enemyRigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

                // 减少敌人生命值
                EnemyControl enemyControl = other.GetComponent<EnemyControl>();
                if (enemyControl != null)
                {
                    enemyControl.health -= 1;
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