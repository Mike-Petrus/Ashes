public readonly struct ActorId
{
    public readonly int Value;

    public ActorId(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public bool Equals(ActorId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ActorId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(ActorId a, ActorId b) => a.Value == b.Value;
    public static bool operator !=(ActorId a, ActorId b) => a.Value != b.Value;
}