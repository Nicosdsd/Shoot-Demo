using UnityEngine;
using UnityEngine.UI;

public class EnemyControl : MonoBehaviour
{
    private Transform player;
    
    public float speed = 5;
    public float health = 2;
    public float maxHealth;
    public Image healthBar;

    public GameObject deathParticlePrefab; // 死亡粒子的预制体

    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>().transform;
        maxHealth = health;
        healthBar.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // 计算敌人与玩家之间的方向向量
        Vector3 direction = (player.transform.position - transform.position).normalized;

        // 移动敌人
        transform.position += direction * speed * Time.deltaTime;

        // 使敌人面朝玩家
        transform.LookAt(player.transform);

        // 更新敌人的血条显示值
        healthBar.fillAmount = health / maxHealth;

        // 检查敌人死亡
        if (health <= 0)
        {
            Die();
        }
    }

    // 敌人死亡
    void Die()
    {
        // 播放一次性死亡粒子效果
        if (deathParticlePrefab != null)
        {
            GameObject particle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            
            // 设置粒子背向玩家
            Vector3 particleDirection = (transform.position - player.position).normalized;
            particle.transform.rotation = Quaternion.LookRotation(particleDirection);
    
            // 销毁粒子对象自身
            Destroy(particle, 2f); // 2秒后销毁实例，以确保粒子播放完毕
        }
    
        Destroy(gameObject); // 立即销毁敌人对象，因为粒子独立存在
    }
}