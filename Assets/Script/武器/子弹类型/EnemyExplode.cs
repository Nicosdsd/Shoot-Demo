using UnityEngine;

//敌人攻击产生的爆炸
public class EnemyExplode : MonoBehaviour
{
    public float damage = 2; 
    public float explosionForce = 15f;    // 爆炸击退力
    public float destroyTime = 1;
    public string weaponSound; //武器音效
    public float blockTime = 0.5f;
    
    private void Start()
    {
        Destroy(gameObject, destroyTime);
        AudioManager.Instance.PlaySound(weaponSound,transform.position);
        Invoke("CloseCollider", 0.1f);

    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
           
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();

            if (enemyRigidbody != null)
            {
                // 计算击退方向
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                
                // 应用击退力
                enemyRigidbody.AddForce(knockbackDirection * explosionForce, ForceMode.Impulse);
            }

            // 减少玩家生命值
            PlayerControl playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.Hit(damage,blockTime);
            }
        }
    
    }

    void CloseCollider()
    {
        GetComponent<Collider>().enabled = false;
    }
}