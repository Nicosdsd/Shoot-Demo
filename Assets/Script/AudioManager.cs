using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    private Dictionary<string, AudioClip> audioClips;
    private AudioSource audioSource;

    void Awake()
    {
        // 确保只有一个实例存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();

        // 初始化音频剪辑字典
        audioClips = new Dictionary<string, AudioClip>();

        // 加载音效资源
        LoadAudioClips();
    }

    // 加载音效资源
    private void LoadAudioClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio"); // 假设你的音效文件存放在Resources/Audio文件夹中
        foreach (var clip in clips)
        {
            audioClips[clip.name] = clip;
        }
    }

    // 播放音效
    public void PlaySound(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            audioSource.PlayOneShot(audioClips[clipName]);
        }
        else
        {
            Debug.LogWarning("音效未找到: " + clipName);
        }
    }

    // 停止当前播放的音效
    public void StopSound()
    {
        audioSource.Stop();
    }

    // 设置音量
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume); // 确保音量在0到1之间
    }
}