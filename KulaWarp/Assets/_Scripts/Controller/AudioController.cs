using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController ac;

    public AudioClip[] bgmPlayList;

    private AudioSource m_audioSource;
    private int m_currentClip = 0;

    // Start is called before the first frame update
    void Awake()
    {
        if (ac == null)
        {
            DontDestroyOnLoad(gameObject); // This object is scene persistent
            ac = this;
        }
        else if (ac != this) Destroy(gameObject);

        // Load references to game objects and components
        LoadComponents();
    }

    void Update()
    {
        if (!m_audioSource.isPlaying && bgmPlayList.Length > 0)
        {
            PlayNextClip();
        }
    }

    void LoadComponents()
    {
        m_audioSource = gameObject.GetComponent<AudioSource>();
    }

    void PlayNextClip()
    {
        // Stop the playback incase the audio source is still playing
        m_audioSource.Stop();

        // Get the next clip from the play list
        m_audioSource.clip = bgmPlayList[(m_currentClip + 1) % bgmPlayList.Length];

        // Start the new playback
        m_audioSource.Play();
    }
}
