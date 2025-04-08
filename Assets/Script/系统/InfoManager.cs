using System;
using UnityEngine;
using UnityEngine.UI;
//管理玩家UI信息

public class InfoManager : MonoBehaviour
{
   [Header("血量")]
   public Slider healthSlider; // 血量进度条
   public Slider healthSliderHUD; // 场景血量进度条
   [Header("经验")]
   public Slider expSlider; // 经验进度条
   public Slider expSliderHUD; // 场景验进度条
   public Text levelText;
   [Header("分数")]
   public Text scoreText;


   
   private void Awake()
   {
      healthSlider.value = 1; 
      healthSliderHUD.value = 1; 
      expSlider.value = 0;
      expSliderHUD.value = 0;

   }

  //更新血量
   public void UpdateHealthUI(float newHealth) // 传入0到1的数值
   {
      healthSlider.value = newHealth; 
      healthSliderHUD.value = newHealth; 
   }
   //更新经验
   public void UpdateExpUI(float newExp,float level) // 传入0到1的数值
   {
      expSlider.value = newExp; 
      expSliderHUD.value = newExp; 
      levelText.text = $"等级: {level}";
   }
   //更新分数
   public void UpdateScore(float newScore)
   {
      scoreText.text = "分数："+ newScore;
   }
   
   
}
