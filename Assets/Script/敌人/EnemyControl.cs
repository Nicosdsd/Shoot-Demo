using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyControl : MonoBehaviour
{
    public float attack = 1;
    public float speed = 5;
    public float rotationSpeed = 5; // 控制旋转跟随速度
    public Vector2 attackForce;

    public float health;
    public float maxHealth = 2;

    public GameObject deathParticlePrefab;

    public GameObject expPrefab;
    private PlayerControl player;//玩家控制器
    private Rigidbody rb;
    //受击闪白
    private float blinkTime = 0.2f;
    public Material blinkMat;
    private Material defaultMat;
    
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();

        // 初始化血量
        health = maxHealth;

        defaultMat = GetComponent<Renderer>().material;
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
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); // 忽略 Y 轴变化
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // 检查敌人是否死亡
        if (health <= 0)
        {
            Die();
        }
    }

    public void Hit(float damage)
    {
        health -= damage;
        GetComponent<Renderer>().material = blinkMat;
        Invoke("LateHit",blinkTime);
        StartCoroutine(LateHit());
    }
    private IEnumerator LateHit()
    {
        yield return new WaitForSeconds(blinkTime);
        GetComponent<Renderer>().material = defaultMat;
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
        
        Vector3 expPos = new Vector3(transform.position.x, transform.position.y-0.3f, transform.position.z);
        Instantiate(expPrefab, expPos, Quaternion.identity);//实例化经验
        Destroy(gameObject); // 销毁敌人对象本身
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // 玩家受到伤害
            player.Hit(attack);

            // 给玩家添加一个向后和向上的力
            Rigidbody playerRb = other.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // 计算一个方向向量，让玩家"弹开"
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                knockbackDirection.y = 0; // 去掉 Y 轴的分量，保证向后的水平力

                float forceStrength = attackForce.x; // 调整这个值来控制水平力度
                float upwardForce = attackForce.y;  // 向上的力大小

                // 添加水平和垂直的力
                Vector3 force = knockbackDirection * forceStrength + Vector3.up * upwardForce;
                playerRb.AddForce(force, ForceMode.Impulse); // Impulse 模式，立即施加冲量
                player.canFire = false;
                player.canMove = false;
                
            }
        }
    }

    /*public void GiftReward()
    {
        foreach (var weaponGift in spawnerManager.weaponGifts)
        {
            // 使用随机数决定是否掉落物品
            if (Random.value < weaponGift.dropChance) // Random.value 返回 [0, 1) 的随机浮点数
            {
                Instantiate(weaponGift.itemPrefab, transform.position, Quaternion.identity); // 在敌人当前位置生成掉落物品
                break; // 掉落一个物品后退出，防止掉落多个
            }
        }
    }*/

}