using UnityEngine;
using UnityEngine.Audio;

public class SetMixerLevels : MonoBehaviour
{
    public AudioMixer mixer;

    public void SetSFXLevel(float level)
    { mixer.SetFloat("SFX", level); }

    public void SetBGMLevel(float level)
    { mixer.SetFloat("BGM", level); }
}
