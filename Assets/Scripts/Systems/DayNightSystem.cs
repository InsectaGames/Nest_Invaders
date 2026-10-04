using System.Collections.Generic;
using UnityEngine;

public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem
{
    private DayTime _dayTime = DayTime.DAY;
    private List<DayTime> phaseOrder = new List<DayTime>(){DayTime.DAY, DayTime.NIGHT};
    private LevelDefinition levelInfo;
    private float _timer;

    public DayNightSystem() => StartDay();

    private void Update()
    {
        _timer -= Time.deltaTime;

        if(_timer == 0)
        {
            UpdateCycle();
        }
    }

    public void ChangeLevel()
    {
        // Cambiar el levelInfo para los nuevos datos.
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
        if(GetRemainingTime() <= 0)
        {
            _dayTime = phaseOrder[(phaseOrder.IndexOf(_dayTime) + 1) % phaseOrder.Count];

            if(_dayTime == DayTime.DAY) StartDay();
            else StartNight();
        }
    }

    public DayTime GetCurrentPhase() => _dayTime;

    public float GetRemainingTime() => _timer;

    public void ForcePhase(DayTime phase)
    {
        _dayTime = phase;

        if(_dayTime == DayTime.DAY) StartDay();
        else StartNight();
    }
}