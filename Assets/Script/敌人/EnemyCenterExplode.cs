using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float detectionRadius = 5.0f; // 攻击范围半径
    public GameObject bombPrefab; // 炸弹预制体
    public Transform bombSpawnPoint; // 炸弹生成位置
    public Animator animator; // 敌人的动画控制器
    public GameObject AreaObject; // 攻击预警
    public float duration = 1f; // 攻击读秒
    public float attackCooldown = 2.0f; // 攻击间隔时间

    private bool isAttacking;
    private bool isInCooldown;
    public EnemyControl enemyControl;

    void Update()
    {
        if (!isAttacking && !isInCooldown)
        {
            DetectPlayer();
        }
    }

    void DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player") && !isAttacking)
            {
                StartCoroutine(PerformAttack());
                break;
            }
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        enemyControl.canMove = false;
        
        // 播放攻击动画
        float animationSpeed = 1 / duration;
        animator.SetFloat("AttackSpeed",animationSpeed ); //动画速率调整
        animator.SetTrigger("Attack"); 

        float elapsedTime = 0f; // 时间计数器

        // 持续缩放物体从0到1
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scaleProgress = elapsedTime / duration; // 计算当前进度（范围0到1）
            AreaObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scaleProgress);
            yield return null; // 等待下一帧
        }

        // 确保最终缩放值为完整的1
        AreaObject.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.1f); // 额外等待以确保缩放动画干净完成

        Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation); // 生成炸弹

        // 重置攻击状态并开始冷却
        isAttacking = false;
        isInCooldown = true;
        AreaObject.transform.localScale = Vector3.zero;
        enemyControl.canMove = true;

        yield return new WaitForSeconds(attackCooldown); // 等待冷却时间

        isInCooldown = false; // 冷却完成，允许再次检测攻击
    }

    void OnDrawGizmosSelected()
    {
        // 在编辑器中绘制检测范围，方便调试
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}