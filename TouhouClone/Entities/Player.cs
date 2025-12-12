using System.Numerics;
using Raylib_cs;
using TouhouClone.Projectiles;

namespace TouhouClone.Entities;

public class Player() : Entity(Game.ScreenCenter, 12, 100, 100, 0)
{
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    private static Player? _instance;

    private float Speed => _baseSpeed * _speedMod;
    private const float _baseSpeed = 2500f;
    private float _speedMod = 1f;

    private const float ShootCooldown = 0.2f;
    private float _remainingShootCooldown;

    private const float InvulnerabilityTime = .1f;
    private float _remainingInvulnerability;


    public static Player GetInstance() => _instance ??= new Player();


    public override void Update(float dt)
    {
        base.Update(dt);
        if (Velocity.Length() <= 0.1f) Velocity = Vector2.Zero;
        Position += Velocity * dt;
        Velocity /= (1 + dt * 5);
        Position = Game.ClampToScreen(Position, Size);
        if (_remainingInvulnerability > 0)
            _remainingInvulnerability -= dt;
        if (_remainingShootCooldown > 0)
            _remainingShootCooldown -= dt;
    }

    public override void Draw()
    {
        Raylib.DrawCircleV(Position, Size * 2, Color.Blue);
        Raylib.DrawCircleV(Position, Size, Color.White);
    }

    private void MakeInvulnerable() => _remainingInvulnerability = InvulnerabilityTime;

    private bool IsInvulnerable() => _remainingInvulnerability > 0;

    public override void TakeDamage(int amount)
    {
        if (IsInvulnerable()) return;
        base.TakeDamage(amount);
        MakeInvulnerable();
    }

    private void Shoot()
    {
        if (_remainingShootCooldown > 0) return;
        Game.SpawnProjectile(new VerticalProjectile(true, Position, -500, Color.Blue, 10));
        _remainingShootCooldown = ShootCooldown;
    }

    public override string ToString() =>
        $"Player(Health: {Health}/{MaxHealth}, Position: {Position}, Velocity: {Velocity}, Speed: {Speed}, Size: {Size}, ForceVelocity: {Velocity})";

    public void HandleInput(float dt)
    {
        var vel = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.Left)) vel.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) vel.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Up)) vel.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Down)) vel.Y += 1;
        if (Raylib.IsKeyDown((KeyboardKey.Space)))
        {
            Shoot();
            _speedMod = 0.6f;
        }else _speedMod = 1f;

        if (!(vel.Length() > 0)) return;
        vel = Vector2.Normalize(vel) * Speed * dt;
        Velocity += vel;
    }
}