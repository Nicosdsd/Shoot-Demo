using System.Collections;
using UnityEngine;

// 主角控制
public class PlayerControl : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 movement;

    [Header("基础")] public float moveSpeed = 5f; // 移动速度
    public float rotationSpeed = 720f; // 旋转速度
    public float health = 5; // 生命值

    public Joystick leftJoystick; // 左摇杆（移动）
    public Joystick rightJoystick; // 右摇杆（瞄准）
    
    [Header("子弹")] public Animator camAnim;
    public float bulletForce = 50f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;

    private int bulletCount;
    public int maxBulletCount = 6;
    private bool canReload = true;

    [Header("开火")] public bool canFire = true;
    public ParticleSystem firePartices;
    public GameObject shellPartices;
    private Animator playerAnim;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    private Transform currentTarget;

    private bool isManualShooting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
       // targetIcon.SetActive(false);
        playerAnim = GetComponent<Animator>();
        bulletCount = maxBulletCount;
    }

    void Update()
    {
        // 左摇杆控制移动输入
        float moveX = leftJoystick.Horizontal;
        float moveZ = leftJoystick.Vertical;
        movement = new Vector3(moveX, 0, moveZ).normalized;

        // 使用右摇杆进入手动瞄准/射击模式
        Vector3 aimDirection = new Vector3(rightJoystick.Horizontal, 0, rightJoystick.Vertical);
        if (aimDirection.magnitude > 0.1f)
        {
            isManualShooting = true;
            currentTarget = null;
            RotateTowardsAimDirection(aimDirection);
            if (canFire && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
            return; // 如果是手动射击，跳过后续逻辑（锁敌等）
        }

        // 如果没有任何操作，则旋转朝向移动方向
        if (!isManualShooting && movement.magnitude > 0.1f)
        {
            RotateTowardsMovementDirection();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    
    void RotateTowardsAimDirection(Vector3 aimDirection)
    {
        float angle = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

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

    void Fire()
    {
        /*if (bulletCount <= 0) return;
        bulletCount--;
        if (bulletCount == 0 && canReload)
        {
            StartCoroutine(Reload());
        }*/
        
        camAnim?.SetTrigger("CameraShakeTrigger");
        playerAnim?.SetTrigger("Fire");
        firePartices?.Play();

        GameObject bulletInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();
        bulletRigidbody.AddForce(bulletSpawnPoint.forward * bulletForce, ForceMode.Impulse);

        GameObject shellInstance = Instantiate(shellPartices, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        shellInstance.transform.parent = null;
        Destroy(shellInstance, 0.5f);
        
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(2); // 装填动画等待2秒
        bulletCount = maxBulletCount;
    }

   
}