using System;
using UnityEngine;

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
