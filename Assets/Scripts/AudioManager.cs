using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Music Source")]
    [Tooltip("AudioSource que reproduce la música de fondo")]
    [SerializeField] private AudioSource musicSource;
    private float musicOriginalVolume = 1f;

    [Header("Audio Pool")]
    [SerializeField] private int poolSize = 10;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int poolIndex = 0;

    private const string SFX_PARAM = "SFX";
    private const string MUSIC_PARAM = "Music";

    private bool isSFXMuted = false;
    private bool isMusicMuted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer no está asignado en el Inspector.");
        }

        // Inicializar Music Source
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
                Debug.LogWarning("No se encontró AudioSource para la música.");
        }
        if (musicSource != null)
            musicOriginalVolume = musicSource.volume;

        // Restaurar estado guardado
        isSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;

        SetVolume(SFX_PARAM, isSFXMuted);
        SetVolume(MUSIC_PARAM, isMusicMuted);

        InitSFXPool();
    }

    private void InitSFXPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("SFX_AudioSource_" + i);
            go.transform.parent = transform;
            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            sfxPool.Add(source);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 0.5f)
    {
        if (isSFXMuted || clip == null) return;

        AudioSource source = sfxPool[poolIndex];
        source.clip = clip;
        source.volume = volume;
        source.Play();

        poolIndex = (poolIndex + 1) % sfxPool.Count;
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        PlayerPrefs.SetInt("SFXMuted", isSFXMuted ? 1 : 0);
        SetVolume(SFX_PARAM, isSFXMuted);
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        SetVolume(MUSIC_PARAM, isMusicMuted);
    }

    private void SetVolume(string parameterName, bool mute)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(parameterName, mute ? -80f : 0f);
        }
    }

    /// <summary>
    /// Desvanece la música a volumen 0 en el tiempo dado, usando tiempo real (ignora timeScale).
    /// </summary>
    public void FadeOutMusic(float duration)
    {
        if (musicSource != null)
            StartCoroutine(FadeMusicTo(0f, duration, useUnscaledTime: true));
    }

    /// <summary>
    /// Restaura la música a su volumen original en el tiempo dado, usando tiempo real (ignora timeScale).
    /// </summary>
    public void FadeInMusic(float duration)
    {
        if (musicSource != null)
            StartCoroutine(FadeMusicTo(musicOriginalVolume, duration, useUnscaledTime: true));
    }

    private IEnumerator FadeMusicTo(float targetVolume, float duration, bool useUnscaledTime)
    {
        if (musicSource == null)
            yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }
}
