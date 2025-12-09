using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public class TracingProjectile(
    Vector2 Position,
    float speed,
    Color color,
    int damage,
    Entities.Entity Target) : Projectile(Position, 2, speed, color, damage)
{
    public override void Update(float dt)
    {
        base.Update(dt);
        var direction = Vector2.Normalize(Target.Position - Position);
        Position += direction * Speed * dt;
    }
}