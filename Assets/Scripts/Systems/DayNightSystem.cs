using System.Collections.Generic;

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

    public DayNightSystem()
    {
        _timer = levelInfo.dayLength;
    }

    public void ChangeLevel()
    {
        // Cambiar el levelInfo para los nuevos datos.
    }

    public void StartDay()
    {
        
    }

    public void StartNight()
    {
        
    }

    public void UpdateCycle()
    {
        if(GetRemainingTime() <= 0)
        {
            _dayTime = phaseOrder[(phaseOrder.IndexOf(_dayTime) + 1) % phaseOrder.Count];
        }
    }

    public DayTime GetCurrentPhase() => _dayTime;

    public float GetRemainingTime() => _timer;

    public void ForcePhase(DayTime phase)
    {
        _dayTime = phase;

        // _timer = forzar reinicio según fase.
    }
}