using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;

namespace TouhouClone.Projectiles;

public class TracingProjectile(
    bool firedByPlayer,
    Vector2 position,
    float speed,
    Color color,
    int damage,
    Entity target,
    float lifeTime,
    int childCount = 8) : Projectile(firedByPlayer, position, 2, speed, color, damage)
{
    private float LifeTime { get; set; } = lifeTime;
    protected Entity? Target = target;

    public override void Update(float dt)
    {
        LifeTime -= dt;
        if (LifeTime <= 0 || Target == null)
        {
            Explode();
            MarkForRemoval();
            return;
        }

        base.Update(dt);
        var direction = Vector2.Normalize(Target.Position - Position);
        Position += direction * Speed * dt;
    }

    protected void Explode()
    {
        int amount = childCount;
        for (int i = 0; i < amount; i++)
        {
            // spawn in a circle around the current position
            var angle = 2 * MathF.PI * ((float)i / amount);
            var pos = Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Game.SpawnProjectile(new TargetedProjectile(FiredByPlayer, Position, pos, Speed * 2f, Color, Damage / 2));
        }
    }

    public override void Draw()
    {
        if (Target == null) return;
        const float blinkPeriod = 0.125f;
        var color = (LifeTime <= 1f && (LifeTime % (blinkPeriod * 2f)) < blinkPeriod) ? Color.White : Color;

        Raylib.DrawCircleV(Position, Size, color);
        var offTowards = Vector2.Normalize(Target.Position - Position) * Size * 2;
        Raylib.DrawLineV(Position - offTowards, Position + offTowards, color);
        var offNormal =
            Vector2.Normalize(new Vector2(-(Target.Position - Position).Y, (Target.Position - Position).X)) * Size * 2;
        Raylib.DrawLineV(Position - offNormal, Position + offNormal, color);
    }
}