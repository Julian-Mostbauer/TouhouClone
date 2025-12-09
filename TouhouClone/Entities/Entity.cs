namespace TouhouClone.Entities;

public abstract class Entity : GameObj
{
    protected int Health;
    protected int MaxHealth;
    public int GetHealth => Health;
    public int GetMaxHealth => MaxHealth;
    public bool IsAlive => Health > 0;
    public int SlamDamage;

    public virtual void TakeDamage(int amount) => Health -= amount;
}

public record BehaviorModel(
    float TargetChange,
    float SpeedChange,
    float ShootChance,
    float PlayerBias,
    float CenterBias)
{
    public static readonly BehaviorModel Default = new(0.02f, 0.01f, 0.02f, -100f, 100f);
    public static readonly BehaviorModel Tackler = new(0.02f, 0.01f, 0.0f, 500f, 100f);
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
