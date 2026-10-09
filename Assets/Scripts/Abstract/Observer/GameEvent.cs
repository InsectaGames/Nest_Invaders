public enum LogicEvent
{
    NULL,
    GAME_START,
    GAME_END,
    TROOP_PLACED,
    RESOURCE_GATHER_1,
    RESOURCE_GATHER_2
}

[System.Serializable]
public struct GameEvent<T>
{
    public LogicEvent logicEvent;
    public T data;

    public GameEvent(LogicEvent le, T info)
    {
        logicEvent = le;
        data = info;
    }

    public static implicit operator GameEvent<T>(LogicEvent logicEvent)
    {
        return new GameEvent<T> 
        { 
            logicEvent = logicEvent,
            data = default
        };
    }
}