using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;

namespace TouhouClone.Projectiles;

public class TracingProjectile(
    Vector2 position,
    float speed,
    Color color,
    int damage,
    Entities.Entity target) : Projectile(position, 2, speed, color, damage)
{
    private float LifeTime { get; set; } = 5f;

    public override void Update(float dt)
    {
        LifeTime -= dt;
        if (LifeTime <= 0)
        {
            Explode();
            MarkForRemoval();
            return;
        }

        base.Update(dt);
        var direction = Vector2.Normalize(target.Position - Position);
        Position += direction * Speed * dt;
    }

    private void Explode()
    {
        const int amount = 8;
        for (int i = 0; i < amount; i++)
        {
            // spawn in a circle around the current position
            var angle =  2 * MathF.PI * ((float)i / amount);
            var pos = Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Game.SpawnProjectile(new TargetedProjectile(Position, pos, Speed * 2f, Color, Damage / 4),
                false);
        }
    }

    public override void Draw()
    {
        Raylib.DrawCircleV(Position, Size, Color);
        var offTowards = Vector2.Normalize(target.Position - Position) * Size * 2;
        Raylib.DrawLineV(Position - offTowards, Position + offTowards, Color);
        var offNormal =
            Vector2.Normalize(new Vector2(-(target.Position - Position).Y, (target.Position - Position).X)) * Size * 2;
        Raylib.DrawLineV(Position - offNormal, Position + offNormal, Color);
    }
}