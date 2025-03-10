using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BuffManager : MonoBehaviour
{
    public BuffData[] buffDatas;
    private BuffData currentBuff;
    public GameObject buffManu;
    public GameObject bombPrefab; //核弹清屏
    private PlayerControl player;
    
    void Awake()
    {
        AssignBuffDataToChildren();
        player = FindAnyObjectByType<PlayerControl>();
    }

    private void OnEnable()
    {
        AssignBuffDataToChildren();
        Time.timeScale = 0;
    }
    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    //洗牌算法
    private void AssignBuffDataToChildren()
    {
        if (buffDatas.Length == 0 ) return;
        
        for (int i = 0; i < buffDatas.Length; i++)
        {
            int randomIndex = Random.Range(i, buffDatas.Length);
            BuffData temp = buffDatas[i];
            buffDatas[i] = buffDatas[randomIndex];
            buffDatas[randomIndex] = temp;
        }
        int index = 0;
        foreach (Transform child in transform)
        {
            if (index >= buffDatas.Length) break; 

            BuffData buff = buffDatas[index];
            child.GetComponentInChildren<Text>().text = buff.buffName;
            child.GetComponent<BuffUI>().buffData = buff; 
            index++;
        }
        
    }
    
}