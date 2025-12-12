using System.Numerics;
using Raylib_cs;
using TouhouClone.Projectiles;

namespace TouhouClone.Entities.Enemies;

public class SimpleBoss(Vector2 position, BehaviorModel behavior, StatModel stats) : Enemy(position, behavior, stats)
{
    private float _timeAlive = 0f;
    private Phase _phase = Phase.A;
    private const float PhaseChangeChance = 1 / 300f;
    private float _remainingPhaseChangeTime = 0f;
    private const float PhaseChangeInterval = 5f;

    private float _remainingShootTimePhaseA = 0f;
    private const float ShootIntervalA = 1.1f;
    private int _shootCountPhaseA = 0;

    private float _remainingShootTimePhaseB = 0f;
    private const float ShootIntervalB = .4f;

    protected override void Movement(float dt)
    {
        if (_phase == Phase.A) MovementPhaseA(dt);
        else MovementPhaseB(dt);
    }

    private void MovementPhaseA(float dt)
    {
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

    private void MovementPhaseB(float dt)
    {
        const float amplitude = 150f;
        const float angularSpeed = 0.6f;
        const float lerpSpeed = 0.5f;
        var target = Position with { X = Game.ScreenCenter.X + amplitude * MathF.Cos(angularSpeed * _timeAlive) };
        var t = MathF.Min(1f, lerpSpeed * dt);
        Position += (target - Position) * t;
    }

    public override void Update(float dt)
    {
        if (!IsAlive) Explosion();
        _timeAlive += dt;
        _remainingPhaseChangeTime -= dt;
        base.Update(dt);
        Shooting(dt);

        if (Game.Random.NextDouble() < PhaseChangeChance && _remainingPhaseChangeTime <= 0)
        {
            _phase = _phase == Phase.A ? Phase.B : Phase.A;
            _remainingPhaseChangeTime = PhaseChangeInterval;
        }
    }

    private void Shooting(float dt)
    {
        if (_phase == Phase.A) ShootingPhaseA(dt);
        else ShootingPhaseB(dt);
    }


    private void ShootingPhaseA(float dt)
    {
        _remainingShootTimePhaseA -= dt;
        if (_remainingShootTimePhaseA > 0f) return;

        foreach (var point in Game.PointsAroundCircle(Position, Size, Math.Min(_shootCountPhaseA, 32)))
            Game.SpawnProjectile(new TracingProjectile(false, point, Stats.ProjectileSpeed, Color.Red,
                Stats.ProjectileDamage, Player.GetInstance(), .5f));

        _remainingShootTimePhaseA = ShootIntervalA;
        _shootCountPhaseA++;
    }

    private void ShootingPhaseB(float dt)
    {
        _remainingShootTimePhaseB -= dt;
        if (_remainingShootTimePhaseB > 0f) return;

        foreach (var point in Game.PointsAroundCircle(Position, Size + 10, 32))
            Game.SpawnProjectile(new TargetedProjectile(false, point, Player.GetInstance().Position,
                Stats.ProjectileSpeed * 4.5f, Color.Red,
                Stats.ProjectileDamage));
        
        _remainingShootTimePhaseB = ShootIntervalB * (1 + Game.NextFloat());
    }

    private void Explosion()
    {
        foreach (var point in Game.PointsAroundCircle(Position, Size, 32))
            Game.SpawnProjectile(new TracingProjectile(false, point, Stats.ProjectileSpeed, Color.Red,
                Stats.ProjectileDamage, Player.GetInstance(), Game.NextFloat() * 3, 14));
    }

    public override void Draw()
    {
        base.Draw();
        Raylib.DrawText(_phase == Phase.A ? "A" : "B", (int)(Position.X - 5), (int)(Position.Y - 20), 12, Color.Black);
    }

    private enum Phase
    {
        A,
        B
    }
}