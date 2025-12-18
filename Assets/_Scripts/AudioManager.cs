using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("SFX Clips - UI")]
    public AudioClip buttonClickSound;
    public AudioClip popSound;
    
    [Header("SFX Clips - Game")]
    public AudioClip winSound;
    public AudioClip lossSound;
    public AudioClip coinSound; 
    public AudioClip pegAppearSound; 

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AudioSource[] sources = GetComponents<AudioSource>();

            if (sources.Length >= 1) 
            {
                musicSource = sources[0];
            }
            else 
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (sources.Length >= 2) 
            {
                sfxSource = sources[1];
            }
            else 
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (clip == null) return;
        
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true; 
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonSound()
    {
        PlaySFX(buttonClickSound);
    }
}