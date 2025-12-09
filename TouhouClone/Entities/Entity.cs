using System.Numerics;

namespace TouhouClone.Entities;

public abstract class Entity(Vector2 position, float size, int maxHealth, int health, int slamDamage) : GameObj(position, size)
{
    public int Health { get; private set; }= health;
    public int MaxHealth { get; private set; } = maxHealth;
    public bool IsAlive => Health > 0;
    public int SlamDamage { get; private set; } = slamDamage;

    public virtual void TakeDamage(int amount) => Health -= amount;
}

public record BehaviorModel(
    float MovementGoalChange,
    float SpeedChange,
    float ShootChance,
    float PlayerBias,
    float CenterBias)
{
    public static readonly BehaviorModel Default = new(0.02f, 0.01f, 0.02f, -100f, 100f);
    public static readonly BehaviorModel Tackler = new(0.06f, 0.08f, 0.0f, 1000f, 200f);
    public static readonly BehaviorModel Scared = new(0.04f, 0.04f, 0.4f, -10000f, 0f);
};

public record StatModel(
    float BaseSpeed,
    float MaxSpeed,
    float MinSpeed,
    float ProjectileSpeed,
    int ProjectileDamage,
    int SlamDamage,
    int MaxHealth,
    int Size);
