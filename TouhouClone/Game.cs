using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;

namespace TouhouClone;

internal static class Game
{
    public static readonly Random Random = new();
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 600;
    private static readonly List<Projectile> ProjectilesFriendly = new(50);
    private static readonly List<Projectile> ProjectilesEnemy = new(100);
    private static readonly List<Entity> Enemies = new(100);

    public static readonly Vector2 ScreenCenter = new(ScreenWidth / 2f, ScreenHeight / 2f);

    public static void Run()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Touhou Clone - Raylib template");
        Raylib.SetTargetFPS(60);

        SpawnEnemy(new Vector2(ScreenWidth / 2f, 100f), BehaviorModel.Default);
        SpawnEnemy(new Vector2(ScreenWidth / 2f, 100f), BehaviorModel.Default);
        SpawnEnemy(new Vector2(ScreenWidth / 2f, 100f), BehaviorModel.Default);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            HandleInput(dt);
            HandleUpdate(dt);
            HandleDraw();
        }

        Raylib.CloseWindow();
    }

    private static void HandleDraw()
    {
        var player = Player.GetInstance();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // HUD
        Raylib.DrawText($"{player.GetHealth} / {player.GetMaxHealth}", 10, 10, 20, Color.Green);
        Raylib.DrawFPS(10, 40);
        Raylib.DrawText(player.ToString(), 10, 60, 10, Color.Green);
        // Player
        player.Draw();

        // Enemies
        Enemies.ForEach(e => e.Draw());

        // Projectiles
        ProjectilesFriendly.ForEach(p => p.Draw());
        ProjectilesEnemy.ForEach(p => p.Draw());

        Raylib.EndDrawing();
    }

    private static void HandleInput(float dt)
    {
        var player = Player.GetInstance();
        var vel = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.Left)) vel.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) vel.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Up)) vel.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.Down)) vel.Y += 1;
        if (Raylib.IsKeyDown((KeyboardKey.Space)))
            player.Shoot();

        if (!(vel.Length() > 0)) return;
        vel = Vector2.Normalize(vel) * player.Speed * dt;
        player.Velocity += vel;
    }

    private static void HandleUpdate(float dt)
    {
        ProjectilesEnemy.RemoveAll(p => !p.IsActive);
        ProjectilesFriendly.RemoveAll(p => !p.IsActive);
        Enemies.RemoveAll(e => !e.IsActive);

        var player = Player.GetInstance();

        player.Update(dt);
        ProjectilesEnemy.ForEach(p => p.Update(dt));
        ProjectilesFriendly.ForEach(p => p.Update(dt));
        Enemies.ForEach(e => e.Update(dt));

        // Handle collisions
        foreach (var enemy in Enemies)
        {
            foreach (var proj in ProjectilesFriendly.Where(proj => enemy.IsColliding(proj)))
            {
                enemy.TakeDamage(proj.Damage);
                proj.MarkForRemoval();
            }

            if (enemy.IsColliding(Player.GetInstance()))
            {
                player.TakeDamage(enemy.SlamDamage);
                var dir = Vector2.Normalize(Player.GetInstance().Position - enemy.Position) * 20f;
                player.ForcePush(dir);
                enemy.ForcePush(-dir);
            }
        }

        foreach (var proj in ProjectilesEnemy.Where(proj => proj.IsColliding(player)))
        {
            player.TakeDamage(5);
            proj.MarkForRemoval();
        }
    }

    public static Vector2 ClampToScreen(Vector2 position, float size)
    {
        position.X = Math.Clamp(position.X, size, ScreenWidth - size);
        position.Y = Math.Clamp(position.Y, size, ScreenHeight - size);
        return position;
    }

    public static void SpawnProjectile(Vector2 position, int projectileDamage, Vector2 velocity,
        bool firedByPlayer)
    {
        var projectileList = firedByPlayer ? ProjectilesFriendly : ProjectilesEnemy;
        var proj = new Projectile
        {
            Position = position,
            Velocity = velocity,
            Size = 5f,
            Damage = projectileDamage,
            Color = firedByPlayer ? Color.Green : Color.Red
        };
        projectileList.Add(proj);
    }

    private static void SpawnEnemy(Vector2 position, BehaviorModel behaviorModel)
    {
        Enemies.Add(
            new SimpleEntity(position, behaviorModel, new StatModel(200f, 300f, 100f, 200f, 5, 10, 200, 20)));
    }
}