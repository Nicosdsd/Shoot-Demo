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

    public Joystick leftJoystick; // 左摇杆（移动）
    public Joystick rightJoystick; // 右摇杆（瞄准）
    
    [Header("武器管理")] 
    public WeaponData[] WeaponDatas;     // 所有武器数据数组
    public WeaponData currentWeapon;     // 当前选中的武器
    public WeaponData defaultWeapon;     // 默认武器
    private int currentAmmoCount = 1;        // 当前武器剩余子弹数量
    private bool canReload = true;       // 是否能够使用装弹
    public Image bulletLimit;            // 子弹 UI 显示进度条
    public Text weaponText;              // 当前选中的武器文本

    [Header("子弹")]
    public Animator camAnim;
    public Transform bulletSpawnPoint;
    public ParticleSystem firePartices;
    public GameObject shellPartices;
    public bool canFire = true;
    private float nextFireTime = 0f;

    [Header("辅助瞄准")]
    private Transform currentTarget;         // 自动瞄准的当前目标
    public float autoAimRadius = 15f;        // 自动瞄准半径（范围）
    public float autoAimAngle = 30f;         // 自动瞄准角度限制

    private bool isManualShooting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentWeapon = defaultWeapon;
    }

    void Update()
    {
        // 左摇杆控制移动输入
        float moveX = leftJoystick.Horizontal + Input.GetAxis("Horizontal");
        float moveZ = leftJoystick.Vertical + Input.GetAxis("Vertical");
        movement = new Vector3(moveX, 0, moveZ).normalized;

        // 使用右摇杆进入手动瞄准/射击模式
        Vector3 aimDirection = new Vector3(rightJoystick.Horizontal, 0, rightJoystick.Vertical);
        if (aimDirection.magnitude > 0.1f)
        {
            isManualShooting = true;
            RotateTowardsAimDirection(aimDirection);

            // 自动寻找瞄准目标
            currentTarget = FindClosestTarget(autoAimRadius, autoAimAngle);

            if (canFire && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + currentWeapon.fireRate; // 根据当前武器的射速调整开火时间
            }
            return; // 如果是手动射击，不旋转到移动方向
        }
        
        if (aimDirection.magnitude <= 0.1f) 
        {
            isManualShooting = false; // 当右摇杆松开，自动返回旋转朝向移动方向
        }

        // 如果没有任何操作，则旋转朝向移动方向
        if (!isManualShooting && movement.magnitude > 0.1f)
        {
            RotateTowardsMovementDirection();
        }

        // 如果没有手动操作，清除当前目标锁定
        currentTarget = null;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    
    //右摇杆
    void RotateTowardsAimDirection(Vector3 aimDirection)
    {
        float angle = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    //左摇杆
    void RotateTowardsMovementDirection()
    {
        if (movement.magnitude > 0)
        {
            float angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    //发射
    void Fire()
    {
        if (currentAmmoCount <= 0 )
        {
            currentWeapon = defaultWeapon;
            bulletLimit.fillAmount = 1; // 重置子弹 UI 显示
            weaponText.text = currentWeapon.weaponName;
        }
        
        if (currentWeapon != defaultWeapon)
        {
            currentAmmoCount--;
            bulletLimit.fillAmount = (float)currentAmmoCount / currentWeapon.ammoCapacity; // 更新UI进度条
        }

        camAnim?.SetTrigger("CameraShakeTrigger");
        firePartices?.Play();

        Vector3 fireDirection;

        // 如果有目标锁定，则朝向目标开火
        if (currentTarget != null)
        {
            fireDirection = (currentTarget.position - bulletSpawnPoint.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(fireDirection);
            transform.rotation = targetRotation;
        }
        else
        {
            fireDirection = transform.forward; // 使用默认方向
        }

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
    //拾取随机武器
    private void EquipRandomWeapon()
    {
        if (WeaponDatas.Length == 0) return;

        int randomIndex = Random.Range(0, WeaponDatas.Length); // 从数组中随机选择一个武器索引
        currentWeapon = WeaponDatas[randomIndex];
        currentAmmoCount = currentWeapon.ammoCapacity;

        if (bulletLimit != null)
        {
            bulletLimit.fillAmount = 1; // 重置子弹 UI 显示
            weaponText.text = currentWeapon.weaponName;
        }
    }

    //辅助瞄准
    private Transform FindClosestTarget(float radius, float maxAngle)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        
        Transform closestTarget = null;
        float closestDistanceSqr = radius * radius;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector3 directionToTarget = hit.transform.position - transform.position;
            float distanceSqrToTarget = directionToTarget.sqrMagnitude;

            // 检查距离和角度条件是否满足
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (distanceSqrToTarget < closestDistanceSqr && angleToTarget <= maxAngle)
            {
                closestDistanceSqr = distanceSqrToTarget;
                closestTarget = hit.transform;
            }
        }

        return closestTarget;
    }
}