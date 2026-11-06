using UnityEngine;

public class UITroopsGenerator : ASubject<GameEvent<int>>
{
    public void UIGenerateEntity(int id)
    {

        Debug.Log("ENVIANDO EVENTO DESDE: " + GetInstanceID());

        GameEvent<int> troopSummon = new GameEvent<int>(
            LogicEvent.TROOP_PLACED,
            id
        );

        UpdateObservers(troopSummon);
    }

    public void TestGenerate()
    {
        Debug.Log("UITroopsGenerator FUNCIONA: " + gameObject.name);
    }
}