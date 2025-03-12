using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyControl : MonoBehaviour
{
    [Header("基础")]
    public float attack = 1;
    public float speed = 5;
    public float rotationSpeed = 5; // 控制旋转跟随速度
    public Vector2 attackForce;
    public float health;
    public float maxHealth = 2;
    public GameObject[] dropItems; //掉落物
    private PlayerControl player;//玩家控制器
    private Rigidbody rb;
    
    [Header("受击")]
    private float blinkTime = 0.2f;
    public Material blinkMat;
    private Material defaultMat;
    public GameObject deathParticlePrefab;

    [Header("发射物")] 
    public bool canShoot;
    public GameObject targetPrefab;      // 目标点
    public GameObject bulletPrefab;  // 子弹的预制体
    public float launchAngle = 45f; // 发射角度
    public float shootInterval = 2;
    public float destoryBullet = 1;

    
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

        // 循环生成掉落物品
        foreach (var item in dropItems)
        {
            if (item != null)
            {
                // 在敌人位置上生成掉落物品，稍微随机化一下位置，避免重叠
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f), 
                    0, 
                    Random.Range(-0.5f, 0.5f)
                );

                Instantiate(item, transform.position + randomOffset, Quaternion.identity);
            }
        }

        AudioManager.Instance.PlaySound("敌人击碎",transform.position);
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
        targetPos.y = 0;
        GameObject targetInstance = Instantiate(targetPrefab, targetPos, Quaternion.identity);

        // 实例化子弹
        GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();

        // 计算发射所需的速度
        Vector3 direction = targetInstance.transform.position - transform.position;
        float h = direction.y;              // 高度差
        direction.y = 0;                    // 水平方向距离
        float distance = direction.magnitude; // 水平方向长度
        float angleRad = Mathf.Deg2Rad * launchAngle;

        // 计算初速度
        float velocity = Mathf.Sqrt(distance * -Physics.gravity.y / Mathf.Sin(2 * angleRad));

        // 计算速度分量
        Vector3 velocityVector = direction.normalized * velocity * Mathf.Cos(angleRad);
        velocityVector.y = velocity * Mathf.Sin(angleRad);
        
        // 应用初速度
        bulletRigidbody.linearVelocity = velocityVector;

        // 销毁临时对象
        Destroy(targetInstance, destoryBullet);
        Destroy(bulletInstance, destoryBullet);
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