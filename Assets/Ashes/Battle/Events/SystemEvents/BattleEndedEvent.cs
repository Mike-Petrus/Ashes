using System.Collections.Generic;

public class BattleEndedEvent : IBattleEvent
{
    public bool BattleWon;
    public Dictionary<string, int> Loot;

    public BattleEndedEvent(bool battleWon, Dictionary<string, int> loot = null)
    {
        BattleWon = battleWon;
        Loot = loot ?? new Dictionary<string, int>();
    }
}