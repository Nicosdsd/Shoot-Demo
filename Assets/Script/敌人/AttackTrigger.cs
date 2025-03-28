using System;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public EnemyControl enemyControl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            enemyControl.Attack(other.gameObject);
        }
    }
}
