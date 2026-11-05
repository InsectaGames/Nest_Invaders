using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct AudioEntry
{
    public string name;       // Nombre del audio.
    public AudioClip clip;    // Clip de audio asociado.
}

public class AudioManager : Singleton<AudioManager>, IObserver<GameEvent<int>>
{
    [Header("Sonidos y Música")]
    [SerializeField] private List<AudioEntry> audioEntries; // Lista configurable desde el inspector.

    private Dictionary<string, AudioClip> audioClips; // Diccionario para buscar clips rápidamente.

    [Header("AudioSources")]
    [SerializeField] private AudioSource audioSource; // AudioSource dedicado a los sonidos del juego.
    [SerializeField] private AudioSource musicSource; // AudioSource dedicado a la música de fondo.

    [Header("Configuración de volumen")]
    [SerializeField, Range(0f, 1f)] private float defaultSFXVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.5f;

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
    }

    private void Start()
    {
        if (Instance != this) return;
        
        CheckReferences();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

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
                sfxSlider.SetValueWithoutNotify(audioSource.volume);
                sfxSlider.onValueChanged.RemoveListener(OnSoundVolumeChange);
                sfxSlider.onValueChanged.AddListener(ActualizarVolumenSFXSinSonido);

                // Detector de "Fin de arrastre" (PointerUp / EndDrag)
                EventTrigger trigger = sfxSliderGO.GetComponent<EventTrigger>();
                if (trigger == null) trigger = sfxSliderGO.AddComponent<EventTrigger>();

                // Limpieza de eventos previos para evitar duplicados.
                trigger.triggers.Clear();

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp; // Se dispara al soltar el slider.
                entry.callback.AddListener((data) => { PlaySound("Test Sound"); });
                trigger.triggers.Add(entry);
            }
        }

        if (musicSliderGO != null)
        {
            musicSlider = musicSliderGO.GetComponent<Slider>();

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(musicSource.volume);
                musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
        }

        Debug.Log($"Slider SFX encontrado: {sfxSlider != null}");
        Debug.Log($"Slider Music encontrado: {musicSlider != null}");
    }

    private void LoadDefaultAudioSettings()
    {
        audioSource.volume = defaultSFXVolume;
        musicSource.volume = defaultMusicVolume;
    }

    public void OnSoundVolumeChange(float value)
    {
        audioSource.volume = value;
        Debug.Log($"Volumen de sonidos: {value}");
        PlaySound("Test Sound");
    }

    public void OnMusicVolumeChanged(float value)
    {
        musicSource.volume = value;
        Debug.Log($"Volumen de música: {value}");
    }

    // Este se ejecuta mientras se desliza (actualiza el volumen visual y del audio sin spamear el sonido).
    private void ActualizarVolumenSFXSinSonido(float value) => audioSource.volume = value;

    // Este método se llamará ÚNICAMENTE cuando el usuario suelte el slider.
    private void OnSoundVolumeChange_OnRelease(float value) => PlaySound("Test Sound");

    #endregion

    #region PATRÓN OBSERVER

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Añadir el AudioManager a lo que sea necesario según la escena.
        CheckReferences();

        if(SceneManager.GetActiveScene().name == "Main_arena_primavera")
        {
            PlayBackgroundMusic("Test Music");
        }
    }

    public void UpdateObserver(GameEvent<int> data)
    {
        // Reproducir sonidos según información recibida.
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
    public bool IsBackgroundMusicPlaying() => musicSource.isPlaying;
    
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
        }
    }

    // Método para pausar la música de fondo.
    public void PauseBackgroundMusic()
    {
        musicSource.Pause();
    }

    // Método para reanudar la música de fondo.
    public void ResumeBackgroundMusic()
    {
        musicSource.UnPause();
    }

    #endregion  
}