using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using HighlightPlus;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 movement;
    private SystemManager systemManager;
    public Animator playerAni;

    [Header("基础")]
    //标识
    private InfoManager infoManager;//玩家UI信息管理
    //材质效果
    public Material blinkMat;
    public HighlightEffect highlightPlus; //用来做Blink
    private Material defaultMat;
    public GameObject HitEffect;
    //移动控制
    public Joystick leftJoystick; // 左摇杆（移动）
    public bool block ;//玩家控制锁定
   // public float blockTime = 0.5f; //锁定时间
    
    
    [Header("移动优化参数")] 
    [SerializeField] float speedLerpUp = 10f;    // 加速系数
    [SerializeField] float speedLerpDown = 20f;   // 减速系数
    private Vector3 currentVelocity; // 当前实际速度
    
    [Header("可增幅属性")]
    public float health = 5; // 生命值
    private float healthMax ; //初始生命
    public float moveSpeed = 5f; // 移动速度
    public float rotationSpeed = 10f; // 旋转速度
    public float levelGet = 1; // 经验获取效率系数
    public float damage = 1; // 伤害系数
    public float fireRate = 1; // 射速系数
    public float ammoCapacity = 1; // 弹药容量系数
    public float ammoReloading = 1; // 上弹速度系数

    
    
    [Header("射击")] 
    public bool canFire = true; //限制武器装弹
    public Weapon currentWeapon;
    public Animator camAnim;
    private WeaponManager weaponManager;
    public Transform weaponPos;
    private Transform currentTarget; // 自动瞄准的当前目标
    public Transform aimIconPrefab; //准星
    private Vector3 fireDirection; //瞄准方向
    public float autoAimRadius = 20; //锁定范围
    float minAimAngleThreshold = 5f; //瞄准允许射击角度
    public GameObject expArea;
    public Slider reloadAmmoUI; //换弹UI
    public float defaultWeaponNum = 1; //当前武器数量
    
    [Header("受伤")] 
    //public float invincibleTime = 0.3f; // 无敌时间
    public float knockbackRadius = 5f; // 击退的影响半径
    public float knockbackForce = 10f; // 击退的力
    private bool isInvincible; // 标志变量，表示当前是否处于无敌状态

    

    void Start()
    {
        healthMax = health;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 防止物理旋转
        aimIconPrefab?.gameObject.SetActive(false); // 初始禁用准星图标
        systemManager = FindAnyObjectByType<SystemManager>();
        defaultMat = GetComponent<Renderer>().material;
        weaponManager = FindAnyObjectByType<WeaponManager>();
        currentWeapon = GetComponentInChildren<Weapon>();
        infoManager = FindAnyObjectByType<InfoManager>();
    }

    void Update()
    {
        if(block) return;
        
        // 左摇杆控制移动输入
        float moveX = leftJoystick.Horizontal + Input.GetAxis("Horizontal");
        float moveZ = leftJoystick.Vertical + Input.GetAxis("Vertical");
        movement = new Vector3(moveX, 0, moveZ).normalized;
            
        // 新增动画控制
        float movementSpeed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude * moveSpeed * 0.006f;
                
        playerAni.SetFloat("Speed", movementSpeed);
        //print("速度" + movementSpeed);
        
        //位移
        Movement();
        
        //索敌
        currentTarget = FindClosestTarget(autoAimRadius);
        
        //转向
        if (currentTarget != null)
        {
            FireAim();
        }
        else
        {
            RotateMovement();
            aimIconPrefab?.gameObject.SetActive(false); // 若无目标，禁用准星图标
        }
        
    }
    
    
    //角色移动
    private void Movement()
    {
        Vector3 targetVelocity = movement * moveSpeed;
        // 水平速度插值
        currentVelocity.x = Mathf.Lerp(
            currentVelocity.x, 
            targetVelocity.x, 
            (movement.magnitude > 0.1f ? speedLerpUp : speedLerpDown) * Time.fixedDeltaTime
        );
        
        currentVelocity.z = Mathf.Lerp(
            currentVelocity.z, 
            targetVelocity.z, 
            (movement.magnitude > 0.1f ? speedLerpUp : speedLerpDown) * Time.fixedDeltaTime
        );
        // 保持垂直速度（重力）
        currentVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = currentVelocity;
    }

    //角色旋转
    void RotateMovement()
    {
        float angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    //瞄准
    private void FireAim()
    {
        // 计算目标方向，并忽略Y轴上的变化
        Vector3 adjustedDirection = currentTarget.position - currentWeapon.bulletSpawnPoint.position;
        adjustedDirection.y = 0; // 忽略Y轴的旋转

        // 归一化方向向量
        fireDirection = adjustedDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);

        // 计算当前角色旋转与目标方向之间的角度差
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // 平滑插值使角色逐渐旋转到目标方向
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    
        if (aimIconPrefab != null)
        {
            aimIconPrefab.gameObject.SetActive(true); // 启用准星图标

            // 获取当前准星的Y轴位置
            float aimIconY = aimIconPrefab.position.y;

            // 更新准星的位置，使其X和Z指向敌人，而Y保持不变
            aimIconPrefab.position = new Vector3(currentTarget.position.x, aimIconY, currentTarget.position.z);
        }
    
        // 当角度差小于设定的阈值，并且可以射击时，才允许开火
        if (angleDifference < minAimAngleThreshold && canFire)
        {
            //开火
            //playerAni?.SetTrigger("Fire");
            //camAnim?.SetTrigger("CameraShakeTrigger"); //震屏
            currentWeapon.Fire(fireDirection);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Gift")) 
        {
            Destroy(other.gameObject); // 销毁奖励物体
        }
    }
    
    //索敌
    private Transform FindClosestTarget(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy") && !hitCollider.GetComponent<EnemyControl>().isDead) // 确保只检查带有 "Enemy" 标签的对象
            {
                Vector3 directionToTarget = hitCollider.transform.position - transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestTarget = hitCollider.transform;
                }
            }
        }

        return closestTarget;
    }
    
    //受伤
    public void Hit(float damage , float blcokTime)
    {
        // 如果角色正处于无敌状态，直接退出
        if (isInvincible)
            return;
        // 触发无敌状态
        StartCoroutine(ActivateInvincibility(blcokTime));
        
        if (health>0)
        {
            block = true;
            health -= damage;
            infoManager.UpdateHealthUI(health / healthMax);
            playerAni?.SetTrigger("Hit");
            camAnim?.SetTrigger("CameraShakeTrigger");
            HitEffect.SetActive(true);
            AudioManager.Instance.PlaySound("主角受伤",transform.position);
            highlightPlus.overlay = 1;//blink
            // 对周围敌人施加击退效果
            ApplyKnockbackToEnemies();
        }
        else
        {
            systemManager.GameOver();
        }
       

    }

    private IEnumerator ActivateInvincibility(float duration)
    {
        isInvincible = true; // 开始无敌
        yield return new WaitForSeconds(duration); // 等待无敌时间
        
        // 恢复正常状态
        block = false;
        isInvincible = false;
        HitEffect.SetActive(false);
        highlightPlus.overlay = 0; 
      
    }
    
    //对周围敌人施加击退效果
    private void ApplyKnockbackToEnemies()
    {
        // 搜索玩家周围一定半径内的敌人
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, knockbackRadius);
        
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                hitCollider.GetComponent<EnemyControl>().Hit(0);
                Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                // 计算击退方向（从玩家到敌人的方向）
                Vector3 knockbackDirection = hitCollider.transform.position - transform.position;
                knockbackDirection.y = 0;  // 忽略y轴的影响
                knockbackDirection.Normalize();
                // 施加击退力
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f); // 设置Gizmos颜色为绿色，透明度为0.25
        // 在玩家的位置绘制球体表示自动锁定区域
        Gizmos.DrawSphere(transform.position, autoAimRadius);
        
        Gizmos.color = new Color(1, 0, 0, 0.25f); // 击退区域
        Gizmos.DrawSphere(transform.position, knockbackRadius);
    }
}