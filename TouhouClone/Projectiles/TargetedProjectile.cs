using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;

namespace TouhouClone.Projectiles;

public class TargetedProjectile(Vector2 spawnPoint, Vector2 targetPoint, float speed, Color color, int damage)
    : Projectile(spawnPoint, 5, speed, color, damage)
{
    private Vector2 Velocity { get; init; } = Vector2.Normalize(targetPoint - spawnPoint) * speed;

    public override void Update(float dt)
    {
        base.Update(dt);
        Position += Velocity * dt;
    }
}