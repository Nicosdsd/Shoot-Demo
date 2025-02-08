using UnityEngine;
using UnityEngine.UI;
//敌人控制
public class EnemyControl : MonoBehaviour
{
    private Transform player;

    public float speed = 5;
    public float health = 2;
    public float maxHealth;
    public Image healthBar;
    /*public GameObject healthBarPos;
    private Camera mainCamera;*/

    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>().transform;
        maxHealth = health;
        healthBar.fillAmount = 1;
       // mainCamera = Camera.main; 
    }

    // Update is called once per frame
    void Update()
    {
        //计算敌人与玩家之间的方向向量
        Vector3 direction = (player.transform.position - transform.position).normalized;
        
        //移动敌人
        transform.position += direction * speed * Time.deltaTime;
        
        //使敌人面朝玩家
        transform.LookAt(player.transform);
        
        //敌人血量
        /*healthBarPos.transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);*/
        healthBar.fillAmount = health / maxHealth;
        
        if (health == 0)
        {
            Destroy(gameObject,0.5f);
        }
    }

}
