using System.Numerics;

namespace TouhouClone.Entities;

using System.Numerics;
using Raylib_cs;
using TouhouClone.Projectiles;

public class Enemy : Entity
{
    protected float _speed = 0;
    protected static readonly Color[] Colors = [Color.Yellow, Color.Orange, Color.Red];
    protected Color Color => Colors[CalculateColorIndex(Health, MaxHealth)];
    protected Vector2 _goal = new(0, 0);
    protected readonly Random _random = Game.Random;
    protected readonly BehaviorModel Behavior;
    protected readonly StatModel Stats;

    protected Enemy(Vector2 position, BehaviorModel behavior, StatModel stats) : base(position, stats.Size, stats.MaxHealth, stats.MaxHealth, stats.SlamDamage)
    {
        _speed = stats.BaseSpeed;
        Behavior = behavior;
        Stats = stats;
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
        Position = Game.ClampToScreen(Position, Size);
    }
    
    protected virtual void Movement(float dt)
    {
        // Choose a new random goal occasionally
        if (_goal.Length() == 0 || _random.NextDouble() < Behavior.MovementGoalChange)
        {
            float x = (float)(_random.NextDouble() * Game.ScreenWidth);
            float y = (float)(_random.NextDouble() * Game.ScreenHeight);
            var point = new Vector2(x, y);
            var toCenter = Vector2.Normalize(Game.ScreenCenter - point);
            var toPlayer = Vector2.Normalize(Player.GetInstance().Position - point);
            var bias = toCenter * Behavior.CenterBias + toPlayer * Behavior.PlayerBias;
            _goal = point + bias;
        }

        // adjust speed based on randomness and health (less health = faster)
        if (_random.NextDouble() < Behavior.SpeedChange)
            _speed += (float)(_random.NextDouble() - ((float)Health / MaxHealth)) * 20f;
        _speed = Math.Clamp(_speed, Stats.MinSpeed, Stats.MaxSpeed);

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