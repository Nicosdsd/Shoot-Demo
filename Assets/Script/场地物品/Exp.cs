using System;
using UnityEngine;
using Random = UnityEngine.Random;
//掉落的经验值
public class Exp : MonoBehaviour
{
    public float flySpeed = 10f; // 飞行速度
    public float maxAngle; // 角度偏移范围
    public float exp = 1; //经验
    private PlayerControl player;
    //private float OffsetAngle;// 额外旋转角度偏移
    private ExpManager expManager;//经验系统
    public bool canFly;
    public bool canGetExp = true;
    
    private void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
        expManager = FindAnyObjectByType<ExpManager>();
        //OffsetAngle = Random.Range(-maxAngle, maxAngle);
    }

    private void Update()
    {
        if (player != null && canFly)
        {
            // 计算指向玩家的完整三维方向
            Vector3 direction = player.transform.position - transform.position;

            // 如果方向向量的长度很短，停止移动 
            if (direction.magnitude < 0.1f)
            {
                return; // 不继续移动，避免悬空停滞
            }

            // 计算从当前对象到玩家的旋转角度
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * flySpeed);

            // 沿着方向向玩家移动
            transform.position += direction.normalized * flySpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //经验范围触发飞行
        if (other.CompareTag("ExpArea"))
        {
            canFly = true;
        }
        
        //经验获取
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            if (canGetExp)
            {
                expManager.GainExperience(exp);//主角获取经验 
            }

            expManager.GainScore(exp);
            AudioManager.Instance.PlaySound("获取经验",transform.position);
        }
    }

    
}