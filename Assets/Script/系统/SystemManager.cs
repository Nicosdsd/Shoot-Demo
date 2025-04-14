using System;
using HighlightPlus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour
{

    public GameObject gameOver;
    public GameObject gameStart;
    private void Awake()
    {
        PauseGame();
        gameStart.SetActive(true);
    }
    
    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void StartScene()
    {
        Time.timeScale = 1;
        RefreshAllHighlights();
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadScene()
    {
        // 获取当前活动的场景，并重新加载它
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Time.timeScale = 1;
    }

    /// <summary>
    /// 加载指定场景
    /// </summary>
    /// <param name="sceneName">要加载的场景名称</param>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f; // 暂停游戏时间
        
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f; // 恢复游戏时间
    }
    
    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOver.SetActive(true);
    }
    
    public void RefreshAllHighlights()
    {
        // 找到场景中所有 Highlight 组件
        var highlightComponents = FindObjectsOfType<HighlightEffect>();
        foreach (var effect in highlightComponents)
        {
            if (effect.gameObject.activeInHierarchy)
            {
                effect.Refresh();        
            }
        }
    }
}
