using UnityEngine; 
using UnityEngine.UI; 
using Random = UnityEngine.Random;

public class PlayerControl : MonoBehaviour { private Rigidbody rb; private Vector3 movement;

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

public Transform aimIconPrefab;          //准星
private Vector3 fireDirection;          //瞄准方向
public float autoAimRadius = 20;        //锁定范围

void Start()
{
    rb = GetComponent<Rigidbody>();
    currentWeapon = defaultWeapon;
    aimIconPrefab?.gameObject.SetActive(false);  // 初始禁用准星图标
}

void Update()
{
    // 左摇杆控制移动输入
    float moveX = leftJoystick.Horizontal + Input.GetAxis("Horizontal");
    float moveZ = leftJoystick.Vertical + Input.GetAxis("Vertical");
    movement = new Vector3(moveX, 0, moveZ).normalized;
    //发射
    if (canFire && Time.time >= nextFireTime)
    {
        Fire();
        nextFireTime = Time.time + currentWeapon.fireRate; // 根据当前武器的射速调整开火时间
    }
    //瞄准
    FireAim();
    //旋转
    RotateTowardsMovementDirection();
}

void FixedUpdate()
{
    rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
}


//左摇杆
void RotateTowardsMovementDirection()
{
    if (movement.magnitude > 0 && currentTarget ==null)
    {
        float angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}

//瞄准
private void FireAim()
{
    // 调用寻敌的方法，并更新当前目标
    currentTarget = FindClosestTarget(autoAimRadius);

    if (currentTarget != null)
    {
        aimIconPrefab?.gameObject.SetActive(true); // 启用准星图标
    
        // 获取当前准星的Y轴位置
        float aimIconY = aimIconPrefab.position.y;
    
        // 更新准星的位置，使其X和Z指向敌人，而Y保持不变
        aimIconPrefab.position = new Vector3(currentTarget.position.x, aimIconY, currentTarget.position.z);

        fireDirection = (currentTarget.position - bulletSpawnPoint.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    else
    {
        aimIconPrefab?.gameObject.SetActive(false); // 若无目标，禁用准星图标

        fireDirection = transform.forward;
    }
}

//发射
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
//拾取随机武器
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

//索敌
private Transform FindClosestTarget(float radius)
{
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
    Transform closestTarget = null;
    float closestDistanceSqr = Mathf.Infinity;

    foreach (Collider hitCollider in hitColliders)
    {
        if (hitCollider.CompareTag("Enemy"))  // 确保只检查带有 "Enemy" 标签的对象
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
}