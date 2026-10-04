using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem
{
    [Header("Configuración")]
    public DayTime _dayTime { get; private set; } = DayTime.DAY;

    private List<DayTime> phaseOrder = new List<DayTime>(){DayTime.DAY, DayTime.NIGHT};

    [SerializeField] private LevelDefinition levelInfo;

    public float _timer { get; private set; }

    public DayNightSystem() => StartDay();

    public void Update()
    {
        _timer -= Time.deltaTime;
        if(_timer == 0) UpdateCycle();
    }

    public void StartDay()
    {
        Debug.Log("Inicia el día.");
        _timer = levelInfo.dayLength;
    }

    public void StartNight()
    {
        Debug.Log("Inicia la noche.");
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