using UnityEngine;

public class UIMenuController : ASubject<GameEvent<string>>
{
    public void OnUIButtonPressed()
    {
        GameEvent<string> soundEvent = new GameEvent<string>(LogicEvent.UI_SOUND, "UI_Sound");
        UpdateObservers(soundEvent);
    }
}