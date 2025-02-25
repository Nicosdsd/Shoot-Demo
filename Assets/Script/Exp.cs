using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Exp : MonoBehaviour
{
    public float flySpeed = 10f; // 飞行速度
    public float maxAngle; // 角度偏移范围
    public float exp = 1; //经验
    private PlayerControl player;
    private float OffsetAngle;// 额外旋转角度偏移
    private PlayerExp playerExp;//经验系统
    
    private void Start()
    {
        player = FindObjectOfType<PlayerControl>();
        playerExp = FindAnyObjectByType<PlayerExp>();
        OffsetAngle = Random.Range(-maxAngle, maxAngle);
    }

    private void Update()
    {
        if (player == null) return; // 确保玩家对象存在

        // 计算指向玩家的方向（基于 XZ 平面）
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0; // 忽略 Y 轴的高度

        // 计算从当前对象到玩家的角度
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, angle + OffsetAngle, 0); // 设置旋转角度（基于 XZ）

        // 沿着方向向玩家移动
        transform.Translate(Vector3.forward * flySpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            playerExp.GainExperience(exp);//主角获取经验
            playerExp.GainScore(exp);
            AudioManager.Instance.PlaySound("获取经验");
        }
    }
}