public abstract class CommandStep
{
    public bool IsFinished { get; protected set; }

    protected BattleContext context;

    public virtual void Start(BattleContext ctx)
    {
        context = ctx;
    }

    public virtual void Update(float deltaTime) { }

    public virtual void Cancel()
    {
        IsFinished = true;
    }
}