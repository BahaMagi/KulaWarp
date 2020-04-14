using UnityEngine;
using UnityEngine.Audio;
using System;

public class SFXController : MonoBehaviour
{
    public AudioClip       rollingClip, impactClip, burstClip;
    public AudioMixerGroup SFXMixerGroup;

    [Serializable]
    public enum SFXClip {Rolling = 0, Impact = 1, Burst = 2};

    private AudioSource[] m_audioSources;

    void Awake()
    {
        m_audioSources = new AudioSource[SFXClip.GetNames(typeof(SFXClip)).Length];

        // Create AudioSources to host the AudioClips
        m_audioSources[(int)SFXClip.Rolling]             = gameObject.AddComponent<AudioSource>();
        m_audioSources[(int)SFXClip.Rolling].clip        = rollingClip;
        m_audioSources[(int)SFXClip.Rolling].playOnAwake = false;
        m_audioSources[(int)SFXClip.Rolling].outputAudioMixerGroup = SFXMixerGroup;
        m_audioSources[(int)SFXClip.Rolling].spatialBlend = 1.0f;


        m_audioSources[(int)SFXClip.Impact]             = gameObject.AddComponent<AudioSource>();
        m_audioSources[(int)SFXClip.Impact].clip        = impactClip;
        m_audioSources[(int)SFXClip.Impact].playOnAwake = false;
        m_audioSources[(int)SFXClip.Impact].loop        = false;
        m_audioSources[(int)SFXClip.Impact].outputAudioMixerGroup = SFXMixerGroup;
        m_audioSources[(int)SFXClip.Rolling].spatialBlend = 1.0f;

        m_audioSources[(int)SFXClip.Burst]             = gameObject.AddComponent<AudioSource>();
        m_audioSources[(int)SFXClip.Burst].clip        = burstClip;
        m_audioSources[(int)SFXClip.Burst].playOnAwake = false;
        m_audioSources[(int)SFXClip.Burst].loop        = false;
        m_audioSources[(int)SFXClip.Burst].outputAudioMixerGroup = SFXMixerGroup;
        m_audioSources[(int)SFXClip.Rolling].spatialBlend = 1.0f;
    }

    public void Play(SFXClip clip)
    { m_audioSources[(int)clip].Play(); }

    public void Stop(SFXClip clip)
    { m_audioSources[(int)clip].Stop(); }
}
