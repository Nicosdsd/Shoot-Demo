using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour
{
    /*private void Awake()
    {
        PauseGame();
    }*/

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadScene()
    {
        // 获取当前活动的场景，并重新加载它
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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


}
