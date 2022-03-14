using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string soundName;
    public int soundIndex;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume;
    [Range(-3f, 3f)] public float pitch;
    public bool playOnAwake;
    [HideInInspector] public AudioSource source;
    public AudioRolloffMode rolloffMode;
    public float maxDistance = 500;
}
