using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//管理切换武器及武器UI
public class WeaponManager : MonoBehaviour
{
    [Header("武器管理")]
    public Weapon[] weapons;
    public Weapon defaultWeapon;
    public Slider bulletLimit; // 子弹 UI 显示进度条
    public Text weaponText; // 当前选中的武器文本
    private PlayerControl player;
    
    private void Awake()
    {
        player = FindObjectOfType<PlayerControl>();

    }

    private void Update()
    {
        UpdateUI();
    }
    
    //切换武器
    /*public void EquipRandomWeapon()
    {
        if (weapons.Length == 0) return;

        int randomIndex = Random.Range(0, weapons.Length); // 从数组中随机选择一个武器索引

        WeaponOut();
        
        // 实例化新武器
        GameObject newWeapon =  Instantiate(weapons[randomIndex].gameObject, player.weaponPos);
        player.currentWeapon = newWeapon.GetComponent<Weapon>();;
        
        if (bulletLimit != null)
        {
            bulletLimit.value = 1; // 重置子弹 UI 显示
            weaponText.text = player.currentWeapon.weaponName;
        }
    }*/
    
    //武器耗尽
    /*public void WeaponOut()
    {
        player.canFire = false;
        //检查并销毁当前武器
        Transform CurrentWeapon = player.currentWeapon.transform;
        CurrentWeapon.parent = null;
        //弹射丢弃武器
        Rigidbody weaponRigidbody = CurrentWeapon.gameObject.AddComponent<Rigidbody>();
        weaponRigidbody.AddForce(Vector3.up * 20f, ForceMode.Impulse);
        
        Destroy(CurrentWeapon.gameObject,1);
        Invoke("ReloadWeapon",0.5f);
    }*/
    
    //切换武器缓冲
    /*void ReloadWeapon()
    {
        player.canFire = true;
    }*/

    public void SelectWeapon(Weapon selectWeapon)
    {
        Destroy(player.currentWeapon.gameObject); 
        Weapon newWeapon =  Instantiate(selectWeapon, player.weaponPos);
        player.currentWeapon = newWeapon;

    }
    
    
    //子弹耗尽切换为默认武器
    /*public void SwithDefaultWeapon()
    {
        WeaponOut();
        Weapon DefaultWeapon = Instantiate(defaultWeapon, player.weaponPos);
        player.currentWeapon = DefaultWeapon;
    }*/
    
    //武器ui显示
    void UpdateUI()
    {
        bulletLimit.value = player.currentWeapon.currentAmmo  / player.currentWeapon.ammoMax; // 更新UI进度条
        weaponText.text = "" + player.currentWeapon.weaponName;
    }
    
}
