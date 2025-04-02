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
    
    private bool isReloading; // 用于跟踪是否正在换弹
    private float reloadCooldownTimer; // 用于计时的变量
    
    void Awake()
    {
        player = FindObjectOfType<PlayerControl>();
        currentAmmo = ammoMax;
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

            if (isAddWeapon )//外置武器不要装弹UI
            {
                return;
            }
            
            player.reloadAmmoUI.value = 1 - reloadCooldownTimer/(reloadTime/player.ammoReloading) + 0.1f;  //换弹进度条
        }
    }


//发射
    public void Fire(Vector3 fireDirection)
    {
        if (isReloading)
        {
            return; // 如果在换弹CD中，不能开火
        }
    
        // 子弹耗光换回默认武器
        if (currentAmmo == 0)
        {
            Invoke("ReloadAmmo", (reloadTime/player.ammoReloading));
            isReloading = true; // 开始换弹
            reloadCooldownTimer = (reloadTime/player.ammoReloading); // 重置换弹CD计时器
            
            if (isAddWeapon )//外置武器不要装弹UI
            {
                return;
            }
            
            player.canFire = false;
            player.reloadAmmoUI.gameObject.SetActive(true);
          
            Invoke("ReloadSound", reloadTime/player.ammoReloading/4); // 防止上弹与发射音效重叠
        }
    
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate / player.fireRate; // 根据当前武器的射速调整开火时间

            // 播放发射特效
            firePartices?.Play();

            // 创建子弹实例
            GameObject bulletInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

            // 调整子弹的朝向（让子弹的 forward 与 fireDirection 对齐）
            bulletInstance.transform.rotation = Quaternion.LookRotation(fireDirection);

            // 添加力推进子弹
            Rigidbody bulletRigidbody = bulletInstance.GetComponent<Rigidbody>();
            bulletRigidbody.AddForce(fireDirection.normalized * speed, ForceMode.Impulse);

            // 弹壳效果
            GameObject shellInstance = Instantiate(shellPartices, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Destroy(shellInstance, 0.5f);

            currentAmmo--;

            if (isAddWeapon )
            {
                AudioManager.Instance.SetVolume(weaponSound, 0.1f);
            }
            
            //AudioManager.Instance.SetVolume(weaponSound, 1 / player.defaultWeaponNum);
            AudioManager.Instance.PlaySound(weaponSound,transform.position);

            /*// 减少当前弹药数量（默认武器不消耗子弹）
            if (player.currentWeapon.tag != "DefaultWeapon")
            {
                currentAmmo--;
            }*/
        
        }
    }

    void ReloadAmmo()
    {
        currentAmmo = ammoMax;
        
        if (isAddWeapon )//外置武器不要装弹UI
        {
            return;
        }
       
        player.canFire = true;
        player.reloadAmmoUI.gameObject.SetActive(false);
       
    }

    void ReloadSound()
    {
        AudioManager.Instance.PlaySound("上弹",transform.position);
    }
}
