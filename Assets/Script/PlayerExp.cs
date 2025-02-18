using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExp : MonoBehaviour
{
    private PlayerControl player;
    
    [Header("经验值系统")]
    public int level = 1; 
    public float currentExp; 
    public float expToNextLevel = 100; 
    public float expGrowthRate = 1.5f; 

    [Header("UI 显示")]
    public Text levelText; 
    public Slider expSlider; 
    public GameObject buffManu;

    public GameObject bombPrefab; //核弹清屏

    public float slowDownDuration = 1f; // 慢速持续时间
    public AnimationCurve slowDownCurve; // 定义一个动画曲线来控制减速过程

    private void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
    }

    public void GainExperience(float amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel *= expGrowthRate;
        UpdateUI();
        
       
      
        //buffManu.SetActive(true);
        // 调用协程来进行缓慢减速
        //StartCoroutine(SlowDownTime());
        SelectUI();
    }

    private void UpdateUI()
    {
        levelText.text = $"等级: {level}";
        expSlider.value = currentExp / expToNextLevel;
    }
    
    


    void SelectUI()
    {
        buffManu.SetActive(true);
        Time.timeScale = 0;
    }

    public void GameReturn()
    {
        buffManu.SetActive(false);
        Time.timeScale = 1;
        Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
    }
    
    private IEnumerator SlowDownTime()
    {
        float elapsedTime = 0;
        float initialTimeScale = Time.timeScale;

        // 使用协程中的时间来逐步改变 Time.timeScale
        while (elapsedTime < slowDownDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 以避免 Time.timeScale 影响计算
            float percentageComplete = elapsedTime / slowDownDuration;
            
            // 根据动画曲线计算新的时间缩放值
            Time.timeScale = Mathf.Lerp(initialTimeScale, 0.1f, slowDownCurve.Evaluate(percentageComplete));
            yield return null; // 等待下一帧
        }

        // 在缓慢结束后恢复正常时间（可选阶段恢复过程）
        Time.timeScale = initialTimeScale;
    }
}