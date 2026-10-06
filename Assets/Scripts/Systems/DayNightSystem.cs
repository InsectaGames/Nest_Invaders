using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float timeScale = 1f;
    public DayTime _dayTime { get; private set; } = DayTime.DAY;

    private List<DayTime> phaseOrder = new List<DayTime>(){DayTime.DAY, DayTime.NIGHT};

    [SerializeField] private LevelDefinition levelInfo;

    private SpriteRenderer gameBG;
    private Color defaultColour;
    [SerializeField] private Color nightColour;

    public float _timer { get; private set; }

    private void Start()
    {
        gameBG = GameObject.Find("BG").GetComponent<SpriteRenderer>();
        defaultColour = gameBG.color;

        StartDay();
    }

    public void Update()
    {
        _timer -= timeScale * Time.deltaTime;
        // Debug.Log(_timer);
        if(_timer <= 0) UpdateCycle();
    }

    public void StartDay()
    {
        // Debug.Log("Inicia el día.");
        gameBG.color = defaultColour;
        _timer = levelInfo.dayLength;
    }

    public void StartNight()
    {
        // Debug.Log("Inicia la noche.");
        gameBG.color = nightColour;
        _timer = levelInfo.nightLength;
    }

    public void UpdateCycle()
    {
        if(_timer <= 0)
        {
            _dayTime = phaseOrder[(phaseOrder.IndexOf(_dayTime) + 1) % phaseOrder.Count];

            if(_dayTime == DayTime.DAY) StartDay();
            else StartNight();
        }
    }

    public void ForcePhase(DayTime phase)
    {
        _dayTime = phase;

        if(_dayTime == DayTime.DAY) StartDay();
        else StartNight();
    }
}