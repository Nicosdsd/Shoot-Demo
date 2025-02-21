using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyControl : MonoBehaviour
{
    public float attack = 1;
    public float speed = 5;
    public float rotationSpeed = 5; // 控制旋转跟随速度

    public float health;
    public float maxHealth = 2;

    public GameObject deathParticlePrefab;

    public GameObject expPrefab;
    private PlayerControl player;//玩家控制器
  
    private Rigidbody rb;
    
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
      
        
        // 初始化血量
        health = maxHealth;


        // 获取自身的 Rigidbody 组件
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody not found! Please attach a Rigidbody component to the enemy.");
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 防止不必要的旋转
        }
    }

    void FixedUpdate() // 推荐使用 FixedUpdate 因为牵涉到物理模拟
    {
        if (player == null) return;

        // 计算敌人与玩家之间的方向向量
        Vector3 direction = (player.transform.position - transform.position).normalized;

        // 使用 AddForce 按方向施加力进行移动（ForceMode.Acceleration 保证恒速）
        //rb.AddForce(direction * speed, ForceMode.Acceleration);
        rb.linearVelocity = direction * speed;
        // 控制敌人朝向玩家（逐步旋转以平滑过渡，看起来更自然）
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // 检查敌人是否死亡
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 播放一次性死亡粒子效果
        if (deathParticlePrefab != null)
        {
            GameObject particle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

            // 设置粒子背向玩家
            Vector3 particleDirection = (transform.position - player.transform.position).normalized;
            particle.transform.rotation = Quaternion.LookRotation(particleDirection);

            // 销毁粒子对象自身
            Destroy(particle, 2f); // 2秒后销毁实例
        }

        Instantiate(expPrefab, transform.position, Quaternion.identity);//实例化经验
        Destroy(gameObject); // 销毁敌人对象本身
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
           player.Injuried(attack);
        }
    }
}