public abstract class CommandStep
{
    public bool IsFinished { get; protected set; }

    public virtual void Start(BattleContext ctx) { }

    public virtual void Update(BattleContext ctx, float deltaTime) { }

    public virtual void Cancel(BattleContext ctx)
    {
        IsFinished = true;
    }
}