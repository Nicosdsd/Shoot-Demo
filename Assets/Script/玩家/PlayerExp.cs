using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
//玩家经验
public class PlayerExp : MonoBehaviour
{
    private PlayerControl player;
    
    [Header("经验值系统")]
    public int level = 1; 
    public float currentExp; 
    public float expToNextLevel = 100; 
    public float expGrowthRate = 1.5f;
    private float localScore;
    
    [Header("UI 显示")]
    public Text levelText; 
    public Slider expSlider; 
    public GameObject buffManu;
    public Text ScoreText;
    
    public GameObject bombPrefab; //核弹清屏
    
    private void Awake()
    {
        player = FindAnyObjectByType<PlayerControl>();
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
       ScoreText.text = "分数："+ localScore;
    }
    

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel *= expGrowthRate;
        UpdateUI();
        Time.timeScale = 0.8f;
        Invoke("BuffActive",1);
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
        
    }

    void BuffActive()
    {
        buffManu.SetActive(true);
        foreach (var exp in FindObjectsOfType<Exp>())
        {
            exp.canFly = true;
        }
    }

    private void UpdateUI()
    {
        levelText.text = $"等级: {level}";
        expSlider.value = currentExp / expToNextLevel;
    }
    
}