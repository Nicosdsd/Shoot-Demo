using System;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private PlayerControl player;
    public float attack = 1; 

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.Hit(attack);
        }
    }
}
