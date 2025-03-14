using UnityEngine;
//增幅Buff，提升百分比数值，单位0.1
[CreateAssetMenu(fileName = "NewBuff", menuName = "Game/BuffData")]
public class BuffData : ScriptableObject
{
    public string buffName;
    public float damageUp = 1;
    public float fireRateUp = 1;
    public float ammoCapacityUp = 1;
    public float levelUp = 1;
    public float ammoReloadingUp = 1;
    public float expAreaUp = 1;
    
    // 机制类Buff:原地生成
    public GameObject addWeapon; // 生成移动炮台
    public GameObject addWeaponType; // 炮台武器类型
    public bool canAddWeapon; 
    

    public void ApplyToPlayer(PlayerControl player)
    {
        player.damage *= damageUp;
        player.fireRate *= fireRateUp;
        player.ammoCapacity *= ammoCapacityUp;
        player.levelGet *= levelUp;
        player.ammoReloading *= ammoReloadingUp;
        player.expArea.transform.localScale *= expAreaUp;
        
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("AddWeapon");
        int addWeaponNum = objectsWithTag.Length;
        
        // 生成炮台，炮台装配
        if (canAddWeapon && addWeapon != null && addWeaponNum <4)
        {
            // 创建炮台和武器实例
            var turret = Instantiate(addWeapon);
            var weapon = Instantiate(addWeaponType, addWeapon.transform.position, Quaternion.identity);

            // 设置炮台上的武器
            var turretControl = turret.GetComponent<TurretControl>();
            turretControl.currentWeapon = weapon.GetComponent<Weapon>();
            turretControl.currentWeapon.isAddWeapon = true;

            // 删除旧的武器挂点内容
            Destroy(turretControl.firePoint.GetChild(0).gameObject);

            // 将新武器挂载到炮台上
            weapon.transform.parent = turret.transform;
            
            
       
        }
    }
    

}

