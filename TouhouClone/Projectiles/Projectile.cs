using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public abstract class Projectile(bool firedByPlayer, Vector2 Position, float size, float speed, Color color, int damage)
    : GameObj(Position, size)
{
    public bool FiredByPlayer { get; init; } = firedByPlayer;
    public float Speed { get; init; } = speed;
    public Color Color { get; init; } = color;
    public int Damage { get; init; } = damage;
    public override void Draw() => Raylib.DrawCircleV(Position, Size, Color);

    public override void Update(float dt)
    {
        base.Update(dt);
        if (!fullyOnScreen()) MarkForRemoval();
    }
}