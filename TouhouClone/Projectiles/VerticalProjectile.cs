using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public class VerticalProjectile(bool firedByPlayer, Vector2 spawnPoint, int size, float speed, Color color, int damage)
    : Projectile(firedByPlayer, spawnPoint, size, speed, color, damage)
{
    private Vector2 Velocity { get; init; } = new(0, speed);

    public override void Update(float dt)
    {
        base.Update(dt);
        Position += Velocity * dt;
    }

    public override void Draw()
    {
        bool facingUp = Velocity.Y < 0;
        int sgn = facingUp ? 1 : -1;
        Raylib.DrawTriangle(
            new Vector2(Position.X, Position.Y - 2*Size * sgn), // Top vertex
            new Vector2(Position.X - Size * sgn, Position.Y + Size * sgn), // Bottom-left vertex
            new Vector2(Position.X + Size * sgn, Position.Y + Size * sgn), // Bottom-right vertex
            Color
        );
    }
}