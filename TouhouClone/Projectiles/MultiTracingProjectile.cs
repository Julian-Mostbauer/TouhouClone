using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;

namespace TouhouClone.Projectiles;

public class MultiTracingProjectile(bool firedByPlayer, Vector2 position, float speed, Color color, int damage, IEnumerable<Entity> targets, float lifeTime, int childCount = 8)
    : TracingProjectile(firedByPlayer, position, speed, color, damage, targets.First(), lifeTime, childCount)
{
    public override void Update(float dt)
    {
        base.Update(dt);
        if (Target is not { IsActive: true })
            Target = GetNextTarget();

        if (Target != null) return;
        Explode();
        MarkForRemoval();
    }

    private Entity? GetNextTarget()
    {
        Entity? closestTarget = null;
        var closestDistance = float.MaxValue;
        foreach (var target in targets)
        {
            if (!target.IsActive) continue;
            var distance = Vector2.Distance(Position, target.Position);
            if (!(distance < closestDistance)) continue;
            closestDistance = distance;
            closestTarget = target;
        }

        return closestTarget; // Fallback to current target if none found
    }
}