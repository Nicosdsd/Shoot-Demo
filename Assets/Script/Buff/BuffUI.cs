using System;
using UnityEngine;
//Buff UI单元素
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

    //执行bufff
    public void SelectBuff()
    {
        buffData.ApplyToPlayer(player);
        transform.parent.gameObject.SetActive(false);
        buffManu.SetActive(false);
        //Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }
}
