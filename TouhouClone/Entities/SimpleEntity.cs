using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Entities;

public class SimpleEntity : Entity
{
    private float _speed;
    private readonly BehaviorModel _behavior;
    private readonly StatModel _stats;
    private static readonly Color[] Colors = [Color.Yellow, Color.Orange, Color.Red];
    private Color Color => Colors[CalculateColorIndex(Health, MaxHealth)];
    private Vector2 _goal = new(0, 0);
    private readonly Random _random = Game.Random;

    public SimpleEntity(Vector2 position, BehaviorModel behavior, StatModel stats)
    {
        Position = position;
        _behavior = behavior;
        Size = stats.Size;
        _speed = stats.BaseSpeed;
        MaxHealth = stats.MaxHealth;
        Health = stats.MaxHealth;
        SlamDamage = stats.SlamDamage;
        _stats = stats;
    }

    private static int CalculateColorIndex(int health, int maxHealth)
    {
        float healthRatio = (float)health / maxHealth;
        int index = (int)((1 - healthRatio) * Colors.Length);
        return Math.Clamp(index, 0, Colors.Length - 1);
    }

    public override void Update(float dt)
    {
        if (!IsAlive)
        {
            MarkForRemoval();
            return;
        }

        base.Update(dt);

        Movement(dt);
        Shooting();
        Position = Game.ClampToScreen(Position, Size);
    }

    private void Shooting()
    {
        if (!(_random.NextDouble() < _behavior.ShootChance)) return;
        var directionToPlayer =
            Vector2.Normalize(Player.GetInstance().Position - Position);
        Game.SpawnProjectile(Position, _stats.ProjectileDamage, directionToPlayer * _stats.ProjectileSpeed, false);
    }

    private void Movement(float dt)
    {
        // Choose a new random goal occasionally
        if (_goal.Length() == 0 || _random.NextDouble() < _behavior.TargetChange)
        {
            float x = (float)(_random.NextDouble() * Game.ScreenWidth);
            float y = (float)(_random.NextDouble() * Game.ScreenHeight);
            var point = new Vector2(x, y);
            var toCenter = Vector2.Normalize(Game.ScreenCenter - point);
            var toPlayer = Vector2.Normalize(Player.GetInstance().Position - point);
            var bias = toCenter * _behavior.CenterBias + toPlayer * _behavior.PlayerBias;
            _goal = point + bias;
        }

        // adjust speed based on randomness and health (less health = faster)
        if (_random.NextDouble() < _behavior.SpeedChange)
            _speed += (float)(_random.NextDouble() - ((float)Health / MaxHealth)) * 20f;
        _speed = Math.Clamp(_speed, _stats.MinSpeed, _stats.MaxSpeed);

        // calculate velocity towards goal
        var dir = _goal - Position;
        var directionToGoal = dir.Length() > 0 ? Vector2.Normalize(dir) : dir;

        var vel = directionToGoal * _speed;
        Position += vel * dt;
    }

    public override void Draw()
    {
        Raylib.DrawCircleV(Position, Size, Color);
        var text = Health.ToString();
        int fontSize = 14;
        int textWidth = Raylib.MeasureText(text, fontSize);
        int x = (int)(Position.X - textWidth / 2f);
        int y = (int)(Position.Y - fontSize / 2f);
        Raylib.DrawText(text, x, y, fontSize, Color.Black);
    }
}