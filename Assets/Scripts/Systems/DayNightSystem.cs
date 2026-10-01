public enum DayTime
{
    DAY,
    NIGHT
}

public class DayNightSystem
{
    private DayTime _dayTime = DayTime.DAY;
    private float _timer;

    public DayNightSystem()
    {
        // _timer = LevelDefinition.dayLength; para la escena.
    }

    public void StartDay()
    {
        
    }

    public void StartNight()
    {
        
    }

    public void UpdateCycle()
    {
        
    }

    public DayTime GetCurrentPhase() => _dayTime;

    public float GetRemainingTime() => _timer;

    public void ForcePhase(DayTime phase)
    {
        _dayTime = phase;

        // _timer = forzar reinicio según fase.
    }
}