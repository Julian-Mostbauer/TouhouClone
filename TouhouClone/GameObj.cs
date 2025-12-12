using System.Numerics;

namespace TouhouClone;

public abstract class GameObj(Vector2 position, float size)
{
    public Vector2 Position { get; protected set; } = position;
    protected readonly float Size = size;
    private Vector2 _forceVelocity = Vector2.Zero;
    public bool IsActive { get; private set; } = true;

    public virtual void Update(float dt)
    {
        if (_forceVelocity.Length() <= 0.1f) return;

        Position += _forceVelocity * dt;
        _forceVelocity /= (1 + dt);
    }

    public abstract void Draw();

    public bool IsColliding(GameObj other)
    {
        float distSq = Vector2.DistanceSquared(Position, other.Position);
        float radiusSum = Size + other.Size;
        return distSq <= radiusSum * radiusSum;
    }

    public bool fullyOnScreen() =>
        Position.X + Size >= 0 && Position.X - Size <= Game.ScreenWidth &&
        Position.Y + Size >= 0 && Position.Y - Size <= Game.ScreenHeight;

    public void ForcePush(Vector2 force) => _forceVelocity += force;

    public void MarkForRemoval() => IsActive = false;
}