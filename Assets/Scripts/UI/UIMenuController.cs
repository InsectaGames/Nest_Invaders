using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : ASubject<GameEvent<string>>
{

    [SerializeField] private Animator _animDisplay;

    private void Start()
    {
        _animDisplay = GetComponent<Animator>();
    }

    public void OnUIButtonPressed()
    {
        GameEvent<string> soundEvent = new GameEvent<string>(LogicEvent.UI_SOUND, "UI_Sound");
        UpdateObservers(soundEvent);
    }

    public void DisplayInfo()
    {

       bool state =  _animDisplay.GetBool("toggle_ui");

        Debug.Log("Muestro informacion" + state);

        _animDisplay.SetBool("toggle_ui", !state);

    }
  
}