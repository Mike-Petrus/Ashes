// Quick Mock Database for Testing
public class MockEnemyDatabase : IEnemyDatabase
{
    public EnemyTemplate GetEnemy(string enemyId)
    {
        if (enemyId == "Goblin_01")
        {
            return new EnemyTemplate
            {
                EnemyId = "Goblin_01",
                DefaultName = "Goblin",
                Radius = 1.2f,
                BaseAttributes = new CoreAttributes 
                { 
                    Strength = 10, Aether = 10, Vitality = 10, Agility = 5, Speed = 8, MoveDistance = 10 
                }
            };
        }
        return null;
    }
}