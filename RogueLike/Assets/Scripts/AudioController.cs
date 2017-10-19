using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public AudioSource audioSource;
    public AudioSource bgSource;
    private void Awake()
    {
        _instance = this;
    }
    //单例模式
    private static AudioController _instance;
    public static  AudioController Instance
    {
        get
        {
            return _instance;
        }
    }
    /// <summary>
    /// 随机播放音效数组中的一个
    /// </summary>
    /// <param name="clips">备选的音效数组</param>
    public void RandomPlay(AudioClip[] clips)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        int index = Random.Range(0, clips.Length);
        audioSource.clip = clips[index];
        audioSource.pitch = pitch;
        audioSource.Play();       
    }
    public void StopBGM()
    {
        bgSource.Stop();
    }
}
