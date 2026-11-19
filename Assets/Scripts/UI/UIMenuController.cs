using UnityEngine;

public class UIMenuController : ASubject<GameEvent<string>>
{
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;

        if (audioManager != null)
            AddObserver(audioManager);
    }

    public void OnUIButtonPressed()
    {
        GameEvent<string> soundEvent = new GameEvent<string>(LogicEvent.UI_SOUND, "UI_Sound");
        UpdateObservers(soundEvent);
    }

    private void OnDestroy()
    {
        if (audioManager != null)
            RemoveObserver(audioManager);
    }
}