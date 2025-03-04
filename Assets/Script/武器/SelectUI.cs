using UnityEngine;
using UnityEngine.UI;

public class SelectUI : MonoBehaviour
{
    private Button button; 
    public Weapon Weapon; // 武器
    private PlayerControl player;
    public Image selectImage;
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
        // 获取 Button 组件
        button = GetComponent<Button>();

        // 注册点击事件
        if (button != null)
        {
            button.onClick.AddListener(SelectWeapon);
        }
    }

    void SelectWeapon()
    {
        Destroy(player.currentWeapon.gameObject); 
        Weapon newWeapon =  Instantiate(Weapon, player.weaponPos);
        player.currentWeapon = newWeapon;
        selectImage.transform.position = transform.position;
    }
}
