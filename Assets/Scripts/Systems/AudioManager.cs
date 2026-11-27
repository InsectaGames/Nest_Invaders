using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

[System.Serializable]
public struct AudioEntry
{
    public string name;       // Nombre del audio.
    public AudioClip clip;    // Clip de audio asociado.
}

public class AudioManager : Singleton<AudioManager>, IObserver<GameEvent<string>>, IObserver<GameEvent<DayTime>>
{
    [Header("Sonidos y Música")]
    [SerializeField] private List<AudioEntry> audioEntries; // Lista configurable desde el inspector.

    private Dictionary<string, AudioClip> audioClips;       // Diccionario para buscar clips rápidamente.

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;       // AudioSource dedicado a los sonidos del juego.
    [SerializeField] private AudioSource musicSource;       // AudioSource dedicado a la música de fondo.
    [SerializeField] private AudioSource dayMusicSource;    // AudioSource dedicado a la música de día.
    [SerializeField] private AudioSource nightMusicSource;  // AudioSource dedicado a la música de noche.
    [SerializeField] private AudioMixer audioMixer;

    [Header("Configuración de volumen")]
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.5f;

    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private float currentSFXVolume;
    private float currentMusicVolume;

    [Header("Transición musical")]
    [SerializeField, Min(0.1f)] private float musicFadeDuration = 3f;

    private Coroutine musicFadeCoroutine;
    private DayTime currentDayTime = DayTime.DAY;

    private Slider sfxSlider;
    private Slider musicSlider;

    #region SETUP

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return; // Si es duplicado, no seguir.

        InitAudioSystem();
        LoadDefaultAudioSettings();
    }

    private void InitAudioSystem()
    {
        audioClips = new Dictionary<string, AudioClip>();

        foreach (var entry in audioEntries)
        {
            if (!audioClips.ContainsKey(entry.name))
                audioClips[entry.name] = entry.clip;
            else
                Debug.LogWarning($"AudioManager: Nombre duplicado '{entry.name}'");
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        dayMusicSource.loop = true;
        dayMusicSource.playOnAwake = false;

        nightMusicSource.loop = true;
        nightMusicSource.playOnAwake = false;
    }

    private void Start()
    {
        if (Instance != this) return;
        
        CheckReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region CONFIGURACIÓN

    private void CheckReferences()
    {
        sfxSlider = null;
        musicSlider = null;

        GameObject sfxSliderGO = null, musicSliderGO = null;

        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (sfxSliderGO == null && go.CompareTag("SFX_Slider")) sfxSliderGO = go;
            if (musicSliderGO == null && go.CompareTag("Music_Slider")) musicSliderGO = go;
            
            if (sfxSliderGO != null && musicSliderGO != null) break;
        }

        if (sfxSliderGO != null)
        {
            sfxSlider = sfxSliderGO.GetComponent<Slider>();

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(currentSFXVolume);
                sfxSlider.onValueChanged.RemoveListener(OnSoundVolumeChange);
                sfxSlider.onValueChanged.AddListener(OnSoundVolumeChange);

                // Detector de "Fin de arrastre" (PointerUp / EndDrag)
                EventTrigger trigger = sfxSliderGO.GetComponent<EventTrigger>();
                if (trigger == null) trigger = sfxSliderGO.AddComponent<EventTrigger>();

                // Limpieza de eventos previos para evitar duplicados.
                trigger.triggers.Clear();

                EventTrigger.Entry soundTrigger = new EventTrigger.Entry();
                soundTrigger.eventID = EventTriggerType.PointerUp; // Se dispara al soltar el slider.
                soundTrigger.callback.AddListener((data) => OnSoundVolumeChange_OnRelease());
                trigger.triggers.Add(soundTrigger);
            }
        }

        if (musicSliderGO != null)
        {
            musicSlider = musicSliderGO.GetComponent<Slider>();

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(currentMusicVolume);
                musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

                EventTrigger musicTrigger = musicSliderGO.GetComponent<EventTrigger>();
                if (musicTrigger == null)
                    musicTrigger = musicSliderGO.AddComponent<EventTrigger>();

                musicTrigger.triggers.Clear();

                EventTrigger.Entry musicEntry = new EventTrigger.Entry();
                musicEntry.eventID = EventTriggerType.PointerUp;
                musicEntry.callback.AddListener((data) => OnMusicVolumeChange_OnRelease());
                musicTrigger.triggers.Add(musicEntry);
            }
        }

        // Debug.Log($"Slider SFX encontrado: {sfxSlider != null}");
        // Debug.Log($"Slider Music encontrado: {musicSlider != null}");

        DayNightSystem dayNightSystem = FindFirstObjectByType<DayNightSystem>();
        UIMenuController uIMenuController = FindFirstObjectByType<UIMenuController>();

        switch(SceneManager.GetActiveScene().name)
        {
            case "Main_arena":
                if(dayNightSystem != null && !dayNightSystem.ContainsObserver(this))
                    dayNightSystem.AddObserver(this);
            break;

            default:
                if (uIMenuController != null && !uIMenuController.ContainsObserver(this))
                    uIMenuController.AddObserver(this);

                if(dayNightSystem != null && dayNightSystem.ContainsObserver(this))
                    dayNightSystem.RemoveObserver(this);
            break;
        }
    }

    private void LoadDefaultAudioSettings()
    {
        currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);

        SetMixerVolume("SFXVolume", currentSFXVolume);
        SetMixerVolume("MusicVolume", currentMusicVolume);
    }

    public void OnSoundVolumeChange(float value)
    {
        currentSFXVolume = value;
        SetMixerVolume("SFXVolume", value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        currentMusicVolume = value;
        SetMixerVolume("MusicVolume", value);
    }

    // Estos métodos se llamarán ÚNICAMENTE cuando el usuario suelte los slider.
    private void OnSoundVolumeChange_OnRelease()
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXVolume);
        PlayerPrefs.Save();

        PlaySound("UI_Sound");
    }

    private void OnMusicVolumeChange_OnRelease()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string parameter, float value)
    {
        float volume = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameter, volume);
    }

    #endregion

    #region DIA Y NOCHE

    private void StartDayNightMusic(DayTime dayTime)
    {
        if (!audioClips.TryGetValue("Day_Theme", out AudioClip dayClip) || !audioClips.TryGetValue("Night_Theme", out AudioClip nightClip))
        {
            Debug.LogWarning("AudioManager: No se han encontrado los temas de día y noche.");
            return;
        }

        currentDayTime = dayTime;

        dayMusicSource.clip = dayClip;
        nightMusicSource.clip = nightClip;

        dayMusicSource.volume = dayTime == DayTime.DAY ? 1f : 0f;
        nightMusicSource.volume = dayTime == DayTime.NIGHT ? 1f : 0f;

        if (!dayMusicSource.isPlaying)
            dayMusicSource.Play();

        if (!nightMusicSource.isPlaying)
            nightMusicSource.Play();
    }

    private void CrossfadeDayNight(DayTime dayTime)
    {
        if (dayTime == currentDayTime)
            return;

        currentDayTime = dayTime;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeDayNightMusic(dayTime));
    }

    private IEnumerator FadeDayNightMusic(DayTime dayTime)
    {
        float startDay = dayMusicSource.volume;
        float startNight = nightMusicSource.volume;

        float targetDay = dayTime == DayTime.DAY ? 1f : 0f;
        float targetNight = dayTime == DayTime.NIGHT ? 1f : 0f;

        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / musicFadeDuration);

            dayMusicSource.volume = Mathf.Lerp(startDay, targetDay, t);
            nightMusicSource.volume = Mathf.Lerp(startNight, targetNight, t);

            yield return null;
        }

        dayMusicSource.volume = targetDay;
        nightMusicSource.volume = targetNight;

        musicFadeCoroutine = null;
    }

    private void StopDayNightMusic()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        dayMusicSource.Stop();
        nightMusicSource.Stop();

        dayMusicSource.clip = null;
        nightMusicSource.clip = null;
    }

    #endregion

    #region PATRÓN OBSERVER

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Añadir el AudioManager a lo que sea necesario según la escena.
        CheckReferences();

        if (SceneManager.GetActiveScene().name == "Main_arena")
        {
            musicSource.Stop();
            StartDayNightMusic(DayTime.DAY);
        }
        else
        {
            StopDayNightMusic();
            PlayBackgroundMusic("Menu_Theme");
        }
    }

    public void UpdateObserver(GameEvent<string> data)
    {
        if (data.logicEvent == LogicEvent.UI_SOUND)
        {
            PlaySound("UI_Sound");
        }
        else if (data.logicEvent == LogicEvent.TROOP_DEATH && data.data.Equals("ALLY"))
        {
            PlaySound("Ally_Death_Sound");
        }
    }

    public void UpdateObserver(GameEvent<DayTime> data)
    {
        CrossfadeDayNight(data.data);
    }

    #endregion

    #region AUDIO

    // Método para reproducir un sonido basado en su nombre.
    public void PlaySound(string clipName, float delay = 0.0f)
    {
        // Si se encuentra el sonido, se reproduce. Si no, salta error.
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            if(delay > 0.0f)
            {
                // Si el delay no es nulo, se reproduce el audio al instante.
                StartCoroutine(WaitAndPlaySound(clip, delay));
            } 
            else if(delay == 0.0f)
            {
                // En caso contrario, esperará el tiempo necesario.
                audioSource.PlayOneShot(clip);
            }
            else
            {
                // En caso de que sea negativo el delay, dará error.
                Debug.LogWarning($"AudioManager: No se puede reproducir un audio con duración '{delay}'");
            }
            
        }
        else
        {
            Debug.LogWarning($"AudioManager: No se encontró un clip con el nombre '{clipName}'.");
        }
    }

    // Método para esperar y reproducir un sonido.
    private IEnumerator WaitAndPlaySound(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        audioSource.PlayOneShot(clip);
    }

    // Método para comprobar si se reproduce la música de fondo.
    public bool IsBackgroundMusicPlaying() =>  musicSource.isPlaying || dayMusicSource.isPlaying || nightMusicSource.isPlaying;
    
    // Método para iniciar la música de fondo.
    public void PlayBackgroundMusic(string musicName)
    {
        if (audioClips.TryGetValue(musicName, out AudioClip musicClip))
        {
            if (musicSource.clip != musicClip || !musicSource.isPlaying)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: No se encontró un clip de música con el nombre '{musicName}'.");
        }
    }

    // Método para detener la música de fondo.
    public void StopBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.clip = null; // Limpia el clip para evitar reinicios accidentales.

            StopDayNightMusic();
        }
    }

    // Método para pausar la música de fondo.
    public void PauseBackgroundMusic()
    {
        musicSource.Pause();
        dayMusicSource.Pause();
        nightMusicSource.Pause();
    }

    // Método para reanudar la música de fondo.
    public void ResumeBackgroundMusic()
    {
        musicSource.UnPause();
        dayMusicSource.UnPause();
        nightMusicSource.UnPause();
    }

    #endregion  
}