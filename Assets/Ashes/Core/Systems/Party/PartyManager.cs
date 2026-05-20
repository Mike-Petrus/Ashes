using System.Collections.Generic;

public class PartyManager
{
    public SharedInventory Inventory { get; private set; } = new();

    private const int MAX_ACTIVE_MEMEBERS = 5;

    private List<PartyMemberData> activeRoster = new();
    private List<PartyMemberData> reserveRoster = new();

    public IReadOnlyList<PartyMemberData> ActiveRoster => activeRoster;
    public IReadOnlyList<PartyMemberData> ReserveRoster => reserveRoster;

    public void AddMemberToParty(PartyMemberData newMember)
    {
        if (activeRoster.Count < MAX_ACTIVE_MEMEBERS)
        {
            activeRoster.Add(newMember);
        }
        else
        {
            reserveRoster.Add(newMember);
        }
    }

    public bool SwapMembers(PartyMemberData activeMember, PartyMemberData reserverMember)
    {
        if (activeRoster.Contains(activeMember) && reserveRoster.Contains(reserverMember))
        {
            activeRoster.Remove(activeMember);
            reserveRoster.Remove(reserverMember);

            activeRoster.Add(reserverMember);
            reserveRoster.Add(activeMember);

            return true;
        }
        return false;
    }

    public void FullHealParty()
    {
        foreach (var member in ActiveRoster)
        {
            member.CurrentHP = member.BaseStats.MaxHP;
            member.CurrentMP = member.BaseStats.MaxMP;
        }
        foreach (var member in ReserveRoster)
        {
            member.CurrentHP = member.BaseStats.MaxHP;
            member.CurrentMP = member.BaseStats.MaxMP;
        }
    }
}