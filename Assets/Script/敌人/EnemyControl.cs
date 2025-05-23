using System;
using System.Collections;
using HighlightPlus;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class EnemyControl : MonoBehaviour
{
    [Header("基础")]
    public Animator playerAni;
    public string speedAnimName;
    public bool canMove;
    
    public float attack = 1;
    public float speed = 5;
    public float rotationSpeed = 5; // 控制旋转跟随速度
    public float health;
    public float maxHealth = 2;
    public GameObject[] dropItems; //掉落物
    private PlayerControl player;//玩家控制器
    private Rigidbody rb;
    
    
    [Header("受击")]
    private float blinkTime = 0.15f;
    /*public Material blinkMat;
    private Material defaultMat;*/
    public HighlightEffect highlightPlus;
    public GameObject deathParticlePrefab;
    public GameObject[] destroyItems; //死亡销毁
    public bool isDead;//死亡约束
    private bool isHit;
    
    
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();

        // 初始化血量
        health = maxHealth;

        //defaultMat = GetComponent<Renderer>().material;
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
    

    void FixedUpdate() 
    {
        if (player == null || isHit || !canMove) return;

        // 计算敌人与玩家之间的方向向量
        Vector3 direction = (player.transform.position - transform.position).normalized;

        // 获取当前速度并保持Y分量，以让重力影响Y轴
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(direction.x * speed, currentVelocity.y, direction.z * speed);

        // 应用新的速度（仅在X和Z轴移动）
        rb.linearVelocity = targetVelocity;

        // 控制敌人朝向玩家（逐步旋转以平滑过渡，看起来更自然）
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); // 忽略 Y 轴变化
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        // 新增动画控制
        
        float movementSpeed = targetVelocity.magnitude * speed * 0.04f;
        playerAni.SetFloat(speedAnimName, movementSpeed);


    }

    public void Hit(float damage)
    {
        health -= damage;
       // playerAni.SetTrigger("Damage");
       isHit = true;
        //GetComponent<Renderer>().material = blinkMat;
        highlightPlus.overlay = 0.95f;
        Invoke("LateHit",blinkTime);
        StartCoroutine(LateHit());
        // 检查敌人是否死亡
        if (health <= 0)
        {
            Die();
        }
        AudioManager.Instance.PlaySound("击中敌人",transform.position);
    }
    private IEnumerator LateHit()
    {
        yield return new WaitForSeconds(blinkTime);
        //GetComponent<Renderer>().material = defaultMat;
        highlightPlus.overlay =  0f;
        isHit = false;
        
    }

    void Die()
    {
        // 死亡约束
        /*if (isDead) return; 
        isDead = true;
        isHit = true;*/
        
        canMove = false;
        
        /*// 添加击飞效果力
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockbackDirection =  (transform.position - player.transform.position).normalized; 
            float knockbackStrength = 100f; // 你可以根据需要调整这个值来控制力量的大小
            rb.AddForce(knockbackDirection * knockbackStrength, ForceMode.Impulse);
        }*/
        
        
        // 播放一次性死亡粒子效果
        if (deathParticlePrefab != null)
        {
            GameObject particle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

            // 设置粒子背向玩家
            Vector3 particleDirection = (transform.position - player.transform.position).normalized;
            particle.transform.rotation = Quaternion.LookRotation(particleDirection);
            particle.transform.parent = null;
            // 销毁粒子对象自身
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

        foreach (GameObject destroyItem in destroyItems)
        {
            if (destroyItems.Length != 0)
            {
                Destroy(destroyItem);
            }
        }

        AudioManager.Instance.PlaySound("敌人击杀",transform.position);
        
        Destroy(gameObject); // 销毁敌人对象本身
        //playerAni.SetBool("Die", true);
    }

    public void Attack(GameObject target)
    {
        playerAni.SetTrigger("Attack");
        // 玩家受到伤害
        player.Hit(attack,0.3f);
    }
    
    /*public IEnumerator ShootingRoutine()
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
        targetPos.y = transform.position.y;  // 确保子弹的发射高度与自身一致

        // 实例化子弹
        GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();

        // 计算发射方向
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;     // 只保留水平分量
        direction.Normalize(); // 归一化，确保方向向量的长度为1
        

        // 应用初速度
        bulletRigidbody.linearVelocity = direction * bulletSpeed;
    }*/
    
}