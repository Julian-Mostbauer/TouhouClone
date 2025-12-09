using System.Numerics;
using Raylib_cs;

namespace TouhouClone;

public class Projectile : GameObj
{
    public Vector2 Velocity { get; init; }
    public Color Color { get; init; }
    public int Damage { get; init; }

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