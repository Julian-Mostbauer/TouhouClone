using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public class VerticalProjectile(bool firedByPlayer, Vector2 spawnPoint, float speed, Color color, int damage)
    : Projectile(firedByPlayer, spawnPoint, 5, speed, color, damage)
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
            new Vector2(Position.X, Position.Y - 10 * sgn), // Top vertex
            new Vector2(Position.X - 5 * sgn, Position.Y + 5 * sgn), // Bottom-left vertex
            new Vector2(Position.X + 5 * sgn, Position.Y + 5 * sgn), // Bottom-right vertex
            Color
        );
    }
}