using System.Numerics;
using TouhouClone.Projectiles;

namespace TouhouClone.Entities.Enemies;

public class SimpleBoss(Vector2 position, BehaviorModel behavior, StatModel stats) : Enemy(position, behavior, stats)
{
    private float _timeAlive = 0f;
    private const float ShootInterval = 1.1f;
    private float _remainingShootTime = 0f;
    private int _shootCount = 0;

    protected override void Movement(float dt)
    {
        _timeAlive += dt;

        const float radius = 100f;
        const float angularSpeed = 1f;
        var target = Game.ScreenCenter + new Vector2(
            radius * MathF.Cos(angularSpeed * _timeAlive),
            radius * MathF.Sin(angularSpeed * _timeAlive)
        ) - new Vector2(0, 150);

        const float lerpSpeed = .5f;
        var t = MathF.Min(1f, lerpSpeed * dt);
        Position += (target - Position) * t;
    }

    public override void Update(float dt)
    {
        if (!IsAlive) Explosion();
        base.Update(dt);
        _remainingShootTime -= dt;
        Shooting();
    }

    private void Shooting()
    {
        if (_remainingShootTime > 0f) return;
        foreach (var point in Game.PointsAroundCircle(Position, Size, Math.Min(_shootCount, 64)))
        {
            Game.SpawnProjectile(new TracingProjectile(false, point, Stats.ProjectileSpeed, Color,
                Stats.ProjectileDamage, Player.GetInstance(), 0.5f));
        }

        _remainingShootTime = ShootInterval;
        _shootCount++;
    }

    private void Explosion()
    {
        foreach (var point in Game.PointsAroundCircle(Position, Size, 32))
        {
            Game.SpawnProjectile(new TracingProjectile(false, point, Stats.ProjectileSpeed, Color,
                Stats.ProjectileDamage, Player.GetInstance(), (float)Game.Random.NextDouble()*3, 14) );
        }
    }
}