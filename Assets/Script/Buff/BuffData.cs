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
    public GameObject layingEggs; // 原地生成物体
    public bool canLayingEggs; 

    public void ApplyToPlayer(PlayerControl player)
    {
        player.damage *= damageUp;
        player.fireRate *= fireRateUp;
        player.ammoCapacity *= ammoCapacityUp;
        player.levelGet *= levelUp;
        player.ammoReloading *= ammoReloadingUp;
        player.expArea.transform.localScale *= expAreaUp;
        
        // 应用机制型Buff逻辑
        if (canLayingEggs && layingEggs != null)
        {
            Instantiate(layingEggs, player.transform.position, Quaternion.identity);
        }
    }
    

}

