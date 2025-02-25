using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;          // 武器名称
    public float damage;               // 武器伤害
    public float fireRate;             // 射速（每秒射击次数）
    public float ammoCapacity;           // 弹药容量
    //public float reloadTime;           // 装填时间
    //public float range;                // 武器射程
    //public GameObject weaponPrefab;    // 武器模型或Prefab
    public GameObject bulletPrefab;    // 子弹模型或Prefab
}