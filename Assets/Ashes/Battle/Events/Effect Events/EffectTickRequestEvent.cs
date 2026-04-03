using System.Collections.Generic;

public class EffectTickRequestEvent : IBattleEvent
{
    public EffectContext Context { get; }
    public List<Effect> Effects { get; }
    public string StatusName { get; } // Good to know WHAT is ticking

    public EffectTickRequestEvent(EffectContext context, List<Effect> effects, string statusName)
    {
        Context = context;
        Effects = effects;
        StatusName = statusName;
    }
}