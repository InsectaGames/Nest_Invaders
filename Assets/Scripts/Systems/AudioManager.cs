using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct AudioEntry
{
    public string name;       // Nombre del audio.
    public AudioClip clip;    // Clip de audio asociado.
}

public class AudioManager : Singleton<AudioManager>, IObserver<GameEvent>
{
    [SerializeField]
    private List<AudioEntry> audioEntries; // Lista configurable desde el inspector.

    private Dictionary<string, AudioClip> audioClips; // Diccionario para buscar clips rápidamente.
    [SerializeField] private AudioSource audioSource; // AudioSource dedicado a los sonidos del juego.
    [SerializeField] private AudioSource musicSource; // AudioSource dedicado a la música de fondo.

    #region SETUP

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return; // Si es duplicado, no seguir.

        InitAudioSystem();

        SceneManager.sceneLoaded += OnSceneLoaded;
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
        PlayBackgroundMusic("Test");
    }

    #endregion

    #region PATRÓN OBSERVER

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Añadir el AudioManager a lo que sea necesario según la escena.
    }

    public void UpdateObserver(GameEvent data)
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