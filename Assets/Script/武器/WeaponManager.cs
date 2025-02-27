using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    [Header("武器管理")]
    public WeaponData[] WeaponDatas; // 所有武器数据数组
    public WeaponData currentWeapon; // 当前选中的武器
    public WeaponData defaultWeapon; // 默认武器
    private float currentAmmoCount = 1; // 当前武器剩余子弹数量
    private bool canReload = true; // 是否能够使用装弹
    public Slider bulletLimit; // 子弹 UI 显示进度条
    public Text weaponText; // 当前选中的武器文本

    [Header("子弹")]
    public Animator camAnim;
    public Transform bulletSpawnPoint;
    public ParticleSystem firePartices;
    public GameObject shellPartices;

    
}
