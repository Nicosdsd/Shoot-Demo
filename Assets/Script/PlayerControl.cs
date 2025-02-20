using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 movement;

    [Header("基础")] 
    public float moveSpeed = 5f; // 移动速度
    public float rotationSpeed = 720f; // 旋转速度
    public float health = 5; // 生命值
    public float level; // 经验
    
    public Joystick leftJoystick; // 左摇杆（移动）
    public Joystick rightJoystick; // 右摇杆（瞄准）
    
    [Header("武器管理")] 
    public WeaponData[] WeaponDatas;     // 所有武器数据数组
    public WeaponData currentWeapon;     // 当前选中的武器
    public WeaponData defaultWeapon;     // 默认武器
    private int currentAmmoCount = 1;        // 当前武器剩余子弹数量
    private bool canReload = true;       // 是否能够使用装弹
    public Slider bulletLimit;            // 子弹 UI 显示进度条
    public Text weaponText;              // 当前选中的武器文本

    [Header("子弹")]
    public Animator camAnim;
    public Transform bulletSpawnPoint;
    public ParticleSystem firePartices;
    public GameObject shellPartices;
    public bool canFire = true;
    private float nextFireTime;

    [Header("辅助瞄准")]
    private Transform currentTarget;         // 自动瞄准的当前目标
    private Transform manualTarget;          // 手动锁定的目标（右摇杆选择）

    public Transform aimIconPrefab;          //准星
    private Vector3 fireDirection;          //瞄准方向
    public float autoAimRadius = 20;        //锁定范围
    
    public float rightJoystickThreshold = 0.5f; // 右摇杆触发阈值
    public float aimAngleThreshold = 45f;   // 锁定角度阈值

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentWeapon = defaultWeapon;
        aimIconPrefab?.gameObject.SetActive(false);  // 初始禁用准星图标
        manualTarget = null; // 初始化没有手动目标
    }

    void Update()
    {
        // 左摇杆控制移动输入
        float moveX = leftJoystick.Horizontal + Input.GetAxis("Horizontal");
        float moveZ = leftJoystick.Vertical + Input.GetAxis("Vertical");
        movement = new Vector3(moveX, 0, moveZ).normalized;

        // 发射
        if (canFire && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + currentWeapon.fireRate; // 根据当前武器的射速调整开火时间
        }

        // 瞄准
        FireAim();

        // 旋转
        RotateTowardsMovementDirection();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    
    
    // 左摇杆控制方向旋转
    void RotateTowardsMovementDirection()
    {
        if (movement.magnitude > 0 && currentTarget == null)
        {
            float angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    // 瞄准逻辑
    private void FireAim()
    {
        // 获取右摇杆输入
        Vector2 rightInput = new Vector2(rightJoystick.Horizontal, rightJoystick.Vertical);

        if (rightInput.magnitude > rightJoystickThreshold)
        {
            // 手动锁定目标
            Vector3 inputDirection = new Vector3(rightInput.x, 0, rightInput.y).normalized;
            manualTarget = FindTargetInDirection(inputDirection);
            currentTarget = manualTarget;   // 将当前目标更新为手动锁定的目标
        }
        else if (manualTarget != null)
        {
            // 如果手动锁定了目标，且右摇杆松开，则保持锁定的手动目标
            currentTarget = manualTarget;
        }
        else
        {
            // 没有手动选择的目标时，默认自动索敌最近目标
            currentTarget = FindClosestTarget(autoAimRadius);
        }

        // 更新准星和射击方向逻辑
        if (currentTarget != null)
        {
            aimIconPrefab?.gameObject.SetActive(true);
            float aimIconY = aimIconPrefab.position.y;
            aimIconPrefab.position = new Vector3(currentTarget.position.x, aimIconY, currentTarget.position.z);

            // 计算射击方向
            fireDirection = (currentTarget.position - bulletSpawnPoint.position).normalized;

            // 平滑转向目标
            Quaternion targetRotation = Quaternion.LookRotation(fireDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            aimIconPrefab?.gameObject.SetActive(false);
            fireDirection = transform.forward;
        }
    }

    // 发射逻辑
    void Fire()
    {
        if (currentAmmoCount <= 0 )
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

        camAnim?.SetTrigger("CameraShakeTrigger");
        
        firePartices?.Play();
        
        GameObject bulletInstance = Instantiate(currentWeapon.bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(fireDirection));
        Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();
        bulletRigidbody.AddForce(fireDirection * 50f, ForceMode.Impulse); // 固定发射力

        GameObject shellInstance = Instantiate(shellPartices, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Destroy(shellInstance, 0.5f);   
    }

   
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("WeaponGift")) // 判断是否碰撞到“WeaponGift”
        {
            EquipRandomWeapon(); // 遇到 WeaponGift 就随机切换武器
            Destroy(other.gameObject); // 销毁奖励物体
        }
    }
    
    private void EquipRandomWeapon()
    {
        if (WeaponDatas.Length == 0) return;

        int randomIndex = Random.Range(0, WeaponDatas.Length); // 从数组中随机选择一个武器索引
        currentWeapon = WeaponDatas[randomIndex];
        currentAmmoCount = currentWeapon.ammoCapacity;

        if (bulletLimit != null)
        {
            bulletLimit.value = 1; // 重置子弹 UI 显示
            weaponText.text = currentWeapon.weaponName;
        }
    }
    
    private Transform FindClosestTarget(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))  
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
    
    private Transform FindTargetInDirection(Vector3 inputDirection)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, autoAimRadius);
        Transform bestTarget = null;
        float bestScore = 0f;

        foreach (Collider hitCollider in hitColliders)
        {
            if (!hitCollider.CompareTag("Enemy")) continue;

            Vector3 toEnemy = (hitCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(inputDirection, toEnemy);

            if (angle < aimAngleThreshold)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float score = (1 - angle / aimAngleThreshold) + (1 - Mathf.Clamp01(distance / autoAimRadius));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = hitCollider.transform;
                }
            }
        }

        return bestTarget ?? FindClosestTarget(autoAimRadius);
    }
}