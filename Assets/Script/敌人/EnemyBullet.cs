using System;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private PlayerControl player;
    public float attack = 1;
    public GameObject boomEffect;
    public float destroyTime = 3;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        boomEffect.SetActive(false);
        Destroy(gameObject,destroyTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        boomEffect.SetActive(true);
        boomEffect.transform.parent = null;
        
        if (other.gameObject.CompareTag("Player"))
        {
            player.Hit(attack);
        }
        AudioManager.Instance.PlaySound("小爆炸",transform.position);
        Destroy(gameObject);
    }
}
