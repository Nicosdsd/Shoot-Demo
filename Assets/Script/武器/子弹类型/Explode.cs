using UnityEngine;
//榴弹爆炸
public class Explode : MonoBehaviour
{
    public int damage = 2; 
    public float explosionForce = 15f;    // 爆炸击退力
    public float destroyTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerStay(Collider other)
    {
        // 检查是否是敌人
        if (other.CompareTag("Enemy"))
        {
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();

            if (enemyRigidbody != null)
            {
                // 计算击退方向
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                
                // 应用击退力
                enemyRigidbody.AddForce(knockbackDirection * explosionForce, ForceMode.Impulse);
            }

            // 减少敌人生命值
            EnemyControl enemyControl = other.GetComponent<EnemyControl>();
            if (enemyControl != null)
            {
                enemyControl.Hit(damage);
            }
        }
    }
}