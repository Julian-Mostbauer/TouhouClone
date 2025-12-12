using System.Numerics;
using Raylib_cs;
using TouhouClone.Projectiles;

namespace TouhouClone.Entities.Enemies;

public class TracingEnemy(Vector2 position, BehaviorModel behavior, StatModel stats) : Enemy(position, behavior, stats)
{
    public override void Update(float dt)
    {
        base.Update(dt);
        Shooting();
    }

    private void Shooting()
    {
        if (!(Game.NextFloat() < Behavior.ShootChance)) return;
        Game.SpawnProjectile(
            new TracingProjectile(false, Position, Stats.ProjectileSpeed, Color.Red,
                Stats.ProjectileDamage, Player.GetInstance(), 5f));
    }
}