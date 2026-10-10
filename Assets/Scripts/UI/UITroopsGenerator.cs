public class UITroopsGenerator : ASubject<GameEvent<int>>
{    
    public void UIGenerateEntity(int id)
    {
        GameEvent<int> troopSummon;
        troopSummon.logicEvent = LogicEvent.TROOP_PLACED;
        troopSummon.data = id;
        UpdateObservers(troopSummon);
    }
}