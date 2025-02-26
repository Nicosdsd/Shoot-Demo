using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 movement;
    private SystemManager systemManager;
    private Animator playerAni;

    [Header("基础")]
    //标识
    public Slider HealthSlider; // 血量进度条
    private bool isInvincible; // 标志变量，表示当前是否处于无敌状态
    public float invincibleTime = 0.2f; // 无敌时间
    //材质效果
    public Material blinkMat;
    private Material defaultMat;
    public GameObject HitEffect;
    //移动控制
    public Joystick leftJoystick; // 左摇杆（移动）
    public bool canMove = true;
    
    [Header("移动优化参数")] 
    [SerializeField] float speedLerpUp = 10f;    // 加速系数
    [SerializeField] float speedLerpDown = 20f;   // 减速系数
    private Vector3 currentVelocity; // 当前实际速度
    
    [Header("可增幅属性")]
    public float health = 5; // 生命值
    private float healthMax ; //初始生命
    public float moveSpeed = 5f; // 移动速度
    public float rotationSpeed = 720f; // 旋转速度
    public float levelUp; // 经验获取效率
    public float damageUp; // 伤害提升
    public float fireRateUp; // 射速提升
    public float ammoCapacityUp; // 弹药容量提升
    
    [Header("武器管理")]
    public WeaponData[] WeaponDatas; // 所有武器数据数组
    public WeaponData currentWeapon; // 当前选中的武器
    public WeaponData defaultWeapon; // 默认武器
    private float currentAmmoCount = 1; // 当前武器剩余子弹数量
    private bool canReload = true; // 是否能够使用装弹
    public Slider bulletLimit; // 子弹 UI 显示进度条
    public Text weaponText; // 当前选中的武器文本

    [Header("子弹")]
    public Animator camAnim;
    public Transform bulletSpawnPoint;
    public ParticleSystem firePartices;
    public GameObject shellPartices;
    public bool canFire = true;
    private float nextFireTime;

    [Header("辅助瞄准")]
    private Transform currentTarget; // 自动瞄准的当前目标
    public Transform aimIconPrefab; //准星
    private Vector3 fireDirection; //瞄准方向
    public float autoAimRadius = 20; //锁定范围
    float minAimAngleThreshold = 5f; //瞄准允许射击角度

    void Start()
    {
        healthMax = health;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 防止物理旋转
        currentWeapon = defaultWeapon;
        aimIconPrefab?.gameObject.SetActive(false); // 初始禁用准星图标
        systemManager = FindAnyObjectByType<SystemManager>();
        playerAni = GetComponent<Animator>();
        defaultMat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (canMove)
        {
            // 左摇杆控制移动输入
            float moveX = leftJoystick.Horizontal + Input.GetAxis("Horizontal");
            float moveZ = leftJoystick.Vertical + Input.GetAxis("Vertical");
            movement = new Vector3(moveX, 0, moveZ).normalized;
        }
        /*//发射
        if (canFire && Time.time >= nextFireTime)
        {
            Fire();
        }*/
        //瞄准
        FireAim();
    }

    void FixedUpdate()
    {
       Movement();
       RotateMovement();
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
        if ( movement.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
       
    }
    //瞄准转向
    void FireRotat()
    {
        if (currentTarget == null || movement.magnitude > 0.1f) return;

        // 计算目标方向
        fireDirection = (currentTarget.position - bulletSpawnPoint.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);

        // 计算当前角色旋转与目标方向之间的角度差
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // 平滑插值使角色逐渐旋转到目标方向
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 当角度差小于设定的阈值，并且可以射击时，才允许开火
        if (angleDifference < minAimAngleThreshold && Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    //瞄准
    private void FireAim()
    {
        FireRotat();
        // 调用寻敌的方法，并更新当前目标
        currentTarget = FindClosestTarget(autoAimRadius);

        if (currentTarget != null)
        {
            aimIconPrefab?.gameObject.SetActive(true); // 启用准星图标

            // 获取当前准星的Y轴位置
            float aimIconY = aimIconPrefab.position.y;

            // 更新准星的位置，使其X和Z指向敌人，而Y保持不变
            aimIconPrefab.position = new Vector3(currentTarget.position.x, aimIconY, currentTarget.position.z);
        }
        else
        {
            aimIconPrefab?.gameObject.SetActive(false); // 若无目标，禁用准星图标
        }
    }

    //发射
    void Fire()
    {
        if (canFire)
        {
            nextFireTime = Time.time + currentWeapon.fireRate * (1 - fireRateUp); // 根据当前武器的射速调整开火时间

            if (currentAmmoCount <= 0)
            {
                currentWeapon = defaultWeapon;
                bulletLimit.value = 1; // 重置子弹 UI 显示
                weaponText.text = currentWeapon.weaponName;
            }

            if (currentWeapon != defaultWeapon)
            {
                currentAmmoCount--;
                bulletLimit.value = (float)currentAmmoCount / currentWeapon.ammoCapacity; // 更新UI进度条
            }

            playerAni?.SetTrigger("Fire");
            camAnim?.SetTrigger("CameraShakeTrigger");
            firePartices?.Play();

            GameObject bulletInstance = Instantiate(currentWeapon.bulletPrefab, bulletSpawnPoint.position,
                Quaternion.LookRotation(fireDirection));
            Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();
            bulletRigidbody.AddForce(fireDirection * 50f, ForceMode.Impulse); // 固定发射力

            GameObject shellInstance = Instantiate(shellPartices, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Destroy(shellInstance, 0.5f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("WeaponGift")) // 判断是否碰撞到“WeaponGift”
        {
            EquipRandomWeapon(); // 遇到 WeaponGift 就随机切换武器
            Destroy(other.gameObject); // 销毁奖励物体
        }
    }

    //拾取随机武器
    private void EquipRandomWeapon()
    {
        if (WeaponDatas.Length == 0) return;

        int randomIndex = Random.Range(0, WeaponDatas.Length); // 从数组中随机选择一个武器索引
        currentWeapon = WeaponDatas[randomIndex];
        currentAmmoCount = currentWeapon.ammoCapacity * (1 + ammoCapacityUp);

        if (bulletLimit != null)
        {
            bulletLimit.value = 1; // 重置子弹 UI 显示
            weaponText.text = currentWeapon.weaponName;
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
            if (hitCollider.CompareTag("Enemy")) // 确保只检查带有 "Enemy" 标签的对象
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
    public void Hit(float damage)
    {
        // 如果角色正处于无敌状态，直接退出
        if (isInvincible)
            return;
        // 触发无敌状态
        StartCoroutine(ActivateInvincibility());
        
        if (health>0)
        {
            health -= damage;
            HealthSlider.value = health / healthMax ;
            playerAni?.SetTrigger("Hit");
            camAnim?.SetTrigger("CameraShakeTrigger");
            HitEffect.SetActive(true);
            AudioManager.Instance.PlaySound("主角受伤");
            GetComponent<Renderer>().material = blinkMat;
        }

    }

    private IEnumerator ActivateInvincibility()
    {
        isInvincible = true; // 开始无敌
        yield return new WaitForSeconds(invincibleTime); // 等待无敌时间
        isInvincible = false; // 恢复正常状态
        HitEffect.SetActive(false);
        canFire = true;
        canMove = true;
        GetComponent<Renderer>().material = defaultMat;
        if (health <= 0)
        {
            systemManager.GameOver();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f); // 设置Gizmos颜色为绿色，透明度为0.25
        // 在玩家的位置绘制球体表示自动锁定区域
        Gizmos.DrawSphere(transform.position, autoAimRadius);
    }
}