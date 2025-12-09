using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Entities;

public class Player : Entity
{
    public Vector2 Velocity;
    private static Player? _instance;
    public readonly float Speed;
    private readonly float _shootCooldown = 0.2f;
    private float _remainingShootCooldown;
    private const float InvulnerabilityTime = .1f;
    private float _remainingInvulnerability;


    public static Player GetInstance() => _instance ??= new Player();

    private Player()
    {
        Size = 12;
        Speed = 2500f;
        MaxHealth = 100;
        Velocity = Vector2.Zero;
        Position = Game.ScreenCenter;
        Health = MaxHealth;
    }

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

    public void Shoot()
    {
        if (_remainingShootCooldown > 0) return;
        Game.SpawnProjectile(Position, 10, new Vector2(0, -500f), true);
        _remainingShootCooldown = _shootCooldown;
    }

    public override string ToString() =>
        $"Player(Health: {Health}/{MaxHealth}, Position: {Position}, Velocity: {Velocity}, Speed: {Speed}, Size: {Size}, ForceVelocity: {Velocity})";
}