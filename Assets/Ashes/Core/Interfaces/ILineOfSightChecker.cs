public interface ILineOfSightChecker
{
    bool HasLineOfSight(SimVector3 origin, SimVector3 target);
}