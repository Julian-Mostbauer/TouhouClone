using System.Numerics;

namespace TouhouClone.Entities.Enemies;

public static class EnemyFactory
{
    public static Enemy CreateSniper(Vector2 position) => new TracingEnemy(position, BehaviorModel.Scared,
        new StatModel(250f, 500f, 200f, 100f, 5, 5, 100, 20));

    public static Entity CreateTank(Vector2 enemySpawn) => new SimpleEnemy(enemySpawn, BehaviorModel.Tackler,
        new StatModel(200f, 300f, 100f, 200f, 5, 10, 200, 50));
}