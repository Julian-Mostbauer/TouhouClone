using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public class VerticalProjectile(Vector2 spawnPoint, float speed, Color color, int damage)
    : Projectile(spawnPoint, 5, speed, color, damage)
{
    private Vector2 Velocity { get; init; } = new(0, speed);

    public override void Update(float dt)
    {
        if (!fullyOnScreen())
        {
            MarkForRemoval();
            return;
        }

        Position += Velocity * dt;
    }

    public override void Draw() => Raylib.DrawCircleV(Position, Size, Color);
}