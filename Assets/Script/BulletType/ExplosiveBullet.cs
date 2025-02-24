using System;
using UnityEngine;
//榴弹子弹
public class ExplosiveBullet : BulletBase
{
    public GameObject explodePrefab;
   private void OnTriggerEnter(Collider other)
   {
       // 如果子弹击中了敌人
       if (other.CompareTag("Enemy"))
       {
           Instantiate(explodePrefab, transform.position, Quaternion.identity);
           Destroy(gameObject);
       }
   }

}
