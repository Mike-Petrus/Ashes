using System;

public struct SimVector3
{
    public float x;
    public float y;
    public float z;

    public SimVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public float Magnitude()
    {
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }

    public SimVector3 Normalized()
    {
        float mag = Magnitude();

        if (mag < 0.0001f)
            return new SimVector3(0, 0, 0);

        return new SimVector3(
            x / mag,
            y / mag,
            z / mag
        );
    }

    public static float Distance(SimVector3 a, SimVector3 b)
    {
        return (a - b).Magnitude();
    }

    public static SimVector3 MoveTowards(
        SimVector3 current,
        SimVector3 target,
        float maxDistanceDelta)
    {
        SimVector3 toTarget = target - current;

        float dist = toTarget.Magnitude();

        if (dist <= maxDistanceDelta || dist == 0f)
            return target;

        return current + toTarget.Normalized() * maxDistanceDelta;
    }

    public static SimVector3 operator +(SimVector3 a, SimVector3 b)
    {
        return new SimVector3(
            a.x + b.x,
            a.y + b.y,
            a.z + b.z
        );
    }

    public static SimVector3 operator -(SimVector3 a, SimVector3 b)
    {
        return new SimVector3(
            a.x - b.x,
            a.y - b.y,
            a.z - b.z
        );
    }

    public static SimVector3 operator *(SimVector3 v, float scalar)
    {
        return new SimVector3(
            v.x * scalar,
            v.y * scalar,
            v.z * scalar
        );
    }

    public static SimVector3 Zero => new SimVector3(0, 0, 0);

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}