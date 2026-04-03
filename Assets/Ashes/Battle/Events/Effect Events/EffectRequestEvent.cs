using System.Collections.Generic;

public class EffectRequestEvent : IBattleEvent
{
    public EffectContext Context { get; }
    public List<Effect> Effects { get; }

    public EffectRequestEvent(EffectContext context, List<Effect> effects)
    {
        Context = context;
        Effects = effects;
    }
}