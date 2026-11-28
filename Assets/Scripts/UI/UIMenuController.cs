using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuController : ASubject<GameEvent<string>> , ISubject<GameEvent<int>>
{
    private List<IObserver<GameEvent<int>>> _observers = new List<IObserver<GameEvent<int>>>();
    [SerializeField] private Animator _animDisplay;

    private void Start()
    {
        _animDisplay = GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name == "Main_arena")
        {
           if(LevelSession.SelectedLevel._tutorial)
            AddObserver(GameObject.FindGameObjectWithTag("tutorial").GetComponent<Tutorial_canvas>());
        }
    }

    public void OnUIButtonPressed()
    {
        GameEvent<string> soundEvent = new GameEvent<string>(LogicEvent.UI_SOUND, "UI_Sound");
        UpdateObservers(soundEvent);
    }

    public void DisplayInfo()
    {
        bool state = _animDisplay.GetBool("toggle_ui");

        Debug.Log("Muestro informacion" + state);

        _animDisplay.SetBool("toggle_ui", !state);

        if (LevelSession.SelectedLevel._tutorial)
            UpdateObservers(new GameEvent<int>(LogicEvent.TUTORIAL_INCREMENT, 0));
    }

    #region OBSERVER

    public void AddObserver(IObserver<GameEvent<int>> obs)
    {
        _observers.Add(obs);
    }

    public void RemoveObserver(IObserver<GameEvent<int>> obs)
    {
        _observers.Remove(obs);
    }

    public void UpdateObservers(GameEvent<int> data)
    {
        foreach (var obs in _observers)
        {
            obs.UpdateObserver(data);
        }
    }

    #endregion
}