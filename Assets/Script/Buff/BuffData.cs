using UnityEngine;
//增幅Buff，提升百分比数值，单位0.1
[CreateAssetMenu(fileName = "NewBuff", menuName = "Game/BuffData")]
public class BuffData : ScriptableObject
{
    public string buffName;
    public float damageUp;
    public float fireRateUp;
    public float ammoCapacityUp;
    public float levelUp;

    public void ApplyToPlayer(PlayerControl player)
    {
        player.damageUp *= damageUp;
        player.fireRateUp *= fireRateUp;
        player.ammoCapacityUp *= ammoCapacityUp;
        player.levelUp *= levelUp;
    }
    

}

