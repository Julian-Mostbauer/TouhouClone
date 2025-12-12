using System.Numerics;

namespace TouhouClone.Entities.Enemies;

public static class EnemyFactory
{
    public static Enemy CreateSniper(Vector2 position) => new TracingEnemy(position, BehaviorModel.Scared,
        new StatModel(250f, 500f, 200f, 100f, 10, 0, 50, 20));

    public static Enemy CreateTank(Vector2 enemySpawn) => new SimpleEnemy(enemySpawn, BehaviorModel.Tackler,
        new StatModel(150f, 500f, 100f, 0f, 0, 10, 200, 50));

    public static Enemy CreateSimple(Vector2 enemySpawn) => new SimpleEnemy(enemySpawn, BehaviorModel.Default,
        new StatModel(200f, 300f, 100f, 200f, 5, 5, 100, 30));

    public static Enemy CreateSimpleBoss(Vector2 enemySpawn) => new SimpleBoss(enemySpawn, BehaviorModel.Default,
        new StatModel(50, 50, 50, 100, 100, 100, 1000, 70));
}