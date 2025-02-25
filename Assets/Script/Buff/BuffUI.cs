using System;
using UnityEngine;

public class BuffUI : MonoBehaviour
{
    public BuffData buffData;
    private PlayerControl player;
    public GameObject buffManu;
    public GameObject bombPrefab; //核弹清屏
    
    private void Awake()
    {
        player = FindAnyObjectByType<PlayerControl>();
    }

    public void SelectBuff()
    {
        buffData.ApplyToPlayer(player);
        transform.parent.gameObject.SetActive(false);
        buffManu.SetActive(false);
        Time.timeScale = 1;
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }
}
