using UnityEngine;
//子弹控制
public class BulletController : MonoBehaviour
{
    public float destroyAfter = 5f;
    public float knockbackForce = 10f;
    public float upwardForce = 5f; // 控制新添加的向上力

    
    void Start()
    {
        Destroy(gameObject, destroyAfter);
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
                // 计算敌人的水平击退方向
                Vector3 knockbackDirection = transform.position - other.transform.position;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();

                // 应用水平击退方向的力
                enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // 应用向上的力
                enemyRigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
            }
           
            // 销毁子弹
            Destroy(gameObject,0.5f);
            
            other.GetComponent<EnemyControl>().health -= 1;
            
           
        }
    }
}