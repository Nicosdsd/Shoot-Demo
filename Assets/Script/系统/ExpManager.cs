using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class ExpManager : MonoBehaviour
{
    // 玩家控制组件（自动关联）
    private PlayerControl player;

    [Header("经验系统配置")]
    public int level = 1;
    public float currentExp;
    public float expToNextLevel = 10;
    public float expGrowthRate = 1.5f;
    private float localScore;

    [Header("UI元素")]
    public Text levelText;
    public Slider expSlider;
    public GameObject buffMenu;
    public Text scoreText;

    [Header("Buff系统")]
    public BuffData[] buffDatas;
    public GameObject bombPrefab;

    [Header("Buff按钮预制件")]
    public Transform buffButtonContainer; // 需要将原buff按钮父节点拖拽至此

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerControl>();
       
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
        foreach (Transform child in buffButtonContainer)
        {
            if (index >= buffDatas.Length) break; 

            BuffData buff = buffDatas[index];
            child.GetComponentInChildren<Text>().text = buff.buffName;
            child.GetComponent<BuffItem>().buffData = buff; 
            index++;
        }
    }
    
    //执行bufff
    public void SelectBuff(GameObject buffItem)
    {
        buffItem.GetComponent<BuffItem>().buffData.ApplyToPlayer(player);
        buffMenu.SetActive(false);
        Time.timeScale = 1;
        //Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }

    public void GainExperience(float amount)
    {
        currentExp += amount * player.levelGet;
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    public void GainScore(float score)
    {
        localScore += score;
        scoreText.text = "分数："+ localScore;
    }
    

    public void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel *= expGrowthRate;
        UpdateUI();
        Time.timeScale = 0.8f;
        Invoke("BuffActive",2);
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
        
    }

    void BuffActive()
    {
        //洗牌并显示三选一
        buffMenu.SetActive(true);
        AssignBuffDataToChildren();
        Time.timeScale = 0;
        //获取场上全部经验
        foreach (var exp in FindObjectsOfType<Exp>())
        {
            exp.canFly = true;
            exp.canGetExp = false;
        }
    }

    private void UpdateUI()
    {
        levelText.text = $"等级: {level}";
        expSlider.value = currentExp / expToNextLevel;
    }
}