using System.Numerics;
using Raylib_cs;

namespace TouhouClone.Projectiles;

public abstract class Projectile(Vector2 Position, float size, float speed, Color color, int damage) : GameObj(Position, size)
{
    public float Speed { get; init; } = speed;
    public Color Color { get; init; } = color;
    public int Damage { get; init; } = damage;
}