using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private Dictionary<string, AudioClip> audioClips;
    private AudioSource audioSource;

    private void Awake()
    {
        // 单例模式确保只有一个 AudioManager 在场景中
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景保留
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        
        // 初始化你的音频剪辑
        audioClips = new Dictionary<string, AudioClip>
        {
            { "Explosion", Resources.Load<AudioClip>("Sounds/Explosion") },
            { "MagicEffect", Resources.Load<AudioClip>("Sounds/MagicEffect") },
            // 在此添加其他音效
        };
    }

    public void PlaySound(string clipName)
    {
        if (!audioClips.ContainsKey(clipName))
        {
            Debug.LogWarning($"Sound '{clipName}' not found!");
            return;
        }

        audioSource.PlayOneShot(audioClips[clipName]);
    }
}