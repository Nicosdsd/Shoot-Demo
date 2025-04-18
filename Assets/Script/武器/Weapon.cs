using Unity.Cinemachine;
using UnityEngine;
//武器发射相关
public class Weapon : MonoBehaviour
{
    [Header("武器管理")]
    public string weaponName;          // 武器名称
    public float damage;               // 武器伤害
    public float speed = 50;            // 子弹速度
    public float fireRate;             // 射速（每秒射击次数）
    public float ammoMax ;           // 弹药容量
    public float currentAmmo ; // 当前武器剩余子弹数量
    public float reloadTime = 1; //上弹时间
    
    [Header("子弹")] 
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public ParticleSystem firePartices; //发射特效
    public GameObject shellPartices; //弹壳特效
    private float nextFireTime;
    private PlayerControl player;
    public bool isAddWeapon;//是否为外置武器
    public string weaponSound; //武器音效

    //增加额外子弹
    public int bulletCount = 1;
    public float spreadAngle = 15;
    
    public float recoilAmount = 0.5f; // 后坐力的偏移量
    public float recoilRecoverySpeed = 5f; // 后坐力恢复速度
    public float shakeForce = 0.1f; // 震屏强度
    private Vector3 originalPosition; // 武器的初始位置
    
    private CinemachineImpulseSource impulseSource;
    private bool isReloading; // 用于跟踪是否正在换弹
    private float reloadCooldownTimer; // 用于计时的变量
    
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
        currentAmmo = ammoMax;
        impulseSource = FindAnyObjectByType<CinemachineImpulseSource>();
        originalPosition = transform.localPosition; // 存储初始本地位置
    }

    void Update()
    {
        // 如果正在进行换弹CD，那么更新计时器
        if (isReloading)
        {
            reloadCooldownTimer -= Time.deltaTime;

            if (reloadCooldownTimer <= 0)
            {
                isReloading = false;
            }
            
            if (!isAddWeapon)
            {
                player.reloadAmmoUI.value = 1 - reloadCooldownTimer/(reloadTime/player.ammoReloading) + 0.1f;  //换弹进度条
            }
           
        }
        
        // 在每帧让武器的位置逐渐恢复到初始位置
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilRecoverySpeed);
    }


//发射
    public void Fire(Vector3 fireDirection)
    {
        if (isReloading)
        {
            return; // 如果在换弹CD中，不能开火
        }

        if (currentAmmo < bulletCount)
        {
            Invoke("ReloadAmmo", (reloadTime / player.ammoReloading));
            isReloading = true; // 开始换弹
            reloadCooldownTimer = (reloadTime / player.ammoReloading); // 重置换弹CD计时器

            if (!isAddWeapon) //外置武器不要装弹UI
            {
                player.reloadAmmoUI.gameObject.SetActive(true);
                Invoke("ReloadSound", reloadTime / player.ammoReloading / 4); // 错开上弹和发射音效
            }
            
        }

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate / player.fireRate; // 根据当前武器的射速调整开火时间

            // 播放发射特效
            firePartices?.Play();

            for (int i = 0; i < bulletCount; i++)
            {
                // 计算当前子弹散射的角度
                float angle = spreadAngle * (i - bulletCount / 2) / bulletCount;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);

                // 创建子弹实例
                GameObject bulletInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

                // 调整子弹的朝向（让子弹的 forward 与经过旋转的 fireDirection 对齐）
                Vector3 spreadDirection = rotation * fireDirection;
                bulletInstance.transform.rotation = Quaternion.LookRotation(spreadDirection);

                // 添加力推进子弹
                Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();
                bulletRigidbody.AddForce(spreadDirection.normalized * speed, ForceMode.Impulse);

                // 弹壳效果
                GameObject shellInstance =
                    Instantiate(shellPartices, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                Destroy(shellInstance, 0.5f);

                currentAmmo--;

                if (isAddWeapon)
                {
                   AudioManager.Instance.SetVolume(weaponSound, 0.2f);
                   AudioManager.Instance.PlaySound(weaponSound, transform.position);
                }
                else
                {
                    AudioManager.Instance.SetVolume(weaponSound, 0.5f);
                    AudioManager.Instance.PlaySound(weaponSound, transform.position);
                }
                
            }

            // 移动武器模型以表现后坐力
            transform.localPosition -= new Vector3(0, 0, recoilAmount);

            //外置武器不震屏
            if (!isAddWeapon)
            {
                impulseSource.GenerateImpulseWithForce(shakeForce);
            }
        }
    }


    void ReloadAmmo()
    {
        currentAmmo = ammoMax;
        
        
        if (!isAddWeapon)
        {
            player.reloadAmmoUI.gameObject.SetActive(false);
        }

       
    }

    void ReloadSound()
    {
        AudioManager.Instance.PlaySound("上弹",transform.position);
    }
}
