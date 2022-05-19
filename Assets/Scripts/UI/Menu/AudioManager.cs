using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public AudioMixerGroup audioMixer;
    public static AudioManager instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.audioClip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = s.playOnAwake;
            s.source.outputAudioMixerGroup = audioMixer;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.soundName == name);
        if(s!= null)
        s.source.Play();
    }

    public void PlayByIndex(int index)
    {
        Sound s = sounds[index];
        s.source.Play();
    }

    public void UpdateVolume(float volume)
    {
        foreach(Sound s in sounds)
        {
            s.volume = volume;
        }
    }
}