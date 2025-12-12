using System.Numerics;
using Raylib_cs;
using TouhouClone.Entities;
using TouhouClone.Entities.Enemies;
using TouhouClone.Projectiles;

namespace TouhouClone;

internal static class Game
{
    public static readonly Random Random = new();
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 800;
    private static List<Projectile> ProjectilesFriendly { get; } = new(50);
    private static List<Projectile> ProjectilesEnemy { get; } = new(100);
    private static List<Projectile> ProjectilesFriendlyQueue { get; } = new(50);
    private static List<Projectile> ProjectilesEnemyQueue { get; } = new(100);
    private static List<Entity> Enemies { get; } = new(100);

    public static readonly Vector2 ScreenCenter = new(ScreenWidth / 2f, ScreenHeight / 2f);
    private static readonly Level Level = Level.LoadFromFile("level1.txt");

    public static void Run()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Touhou Clone - Raylib template");
        Raylib.SetTargetFPS(60);
        Console.WriteLine(Level.Serialize());
        bool finished = false;
        int gameRes = 0;
        while (!Raylib.WindowShouldClose() && !finished)
        {
            var dt = Raylib.GetFrameTime();

            HandleInput(dt);
            HandleUpdate(dt);
            HandleDraw();

            if (Level.Completed && Enemies.Count == 0) gameRes = Player.GetInstance().IsAlive ? 1 : -1;
            if (gameRes != 0 && ProjectilesEnemy.Count == 0) finished = true; // stop when all enemy projectiles are gone
        }

        switch (gameRes)
        {
            case 1:
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText("You Win!", ScreenWidth / 2 - 50, ScreenHeight / 2 - 10, 20, Color.Green);
                Raylib.EndDrawing();
                Raylib.WaitTime(2f);
                break;
            case -1:
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText("Game Over!", ScreenWidth / 2 - 50, ScreenHeight / 2 - 10, 20, Color.Red);
                Raylib.EndDrawing();
                Raylib.WaitTime(2f);
                break;
        }

        Raylib.CloseWindow();
    }

    private static void HandleDraw()
    {
        var player = Player.GetInstance();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Projectiles
        ProjectilesFriendly.ForEach(p => p.Draw());
        ProjectilesEnemy.ForEach(p => p.Draw());

        // Player
        player.Draw();

        // Enemies
        Enemies.ForEach(e => e.Draw());

        // HUD
        Raylib.DrawText($"{player.Health} / {player.MaxHealth}", 10, 10, 20, Color.Green);
        Raylib.DrawText(Level.GetCurrentWaveInfo(), 10, 30, 20, Color.Yellow);


        Raylib.EndDrawing();
    }

    private static void HandleInput(float dt)
    {
        Player.GetInstance().HandleInput(dt);
    }

    private static void HandleUpdate(float dt)
    {
        // remove old
        ProjectilesEnemy.RemoveAll(p => !p.IsActive);
        ProjectilesFriendly.RemoveAll(p => !p.IsActive);
        Enemies.RemoveAll(e => !e.IsActive);

        // add queued
        ProjectilesEnemy.AddRange(ProjectilesEnemyQueue);
        ProjectilesEnemyQueue.Clear();
        ProjectilesFriendly.AddRange(ProjectilesFriendlyQueue);
        ProjectilesFriendlyQueue.Clear();

        // update all projectiles
        ProjectilesEnemy.ForEach(p => p.Update(dt));
        ProjectilesFriendly.ForEach(p => p.Update(dt));

        // update player
        var player = Player.GetInstance();
        player.Update(dt);

        // collisions with enemy projectiles
        foreach (var proj in ProjectilesEnemy.Where(proj => proj.IsColliding(player)))
        {
            player.TakeDamage(5);
            proj.MarkForRemoval();
        }

        // update enemies
        foreach (var enemy in Enemies)
        {
            enemy.Update(dt);
            // collisions with friendly projectiles
            foreach (var proj in ProjectilesFriendly.Where(proj => enemy.IsColliding(proj)))
            {
                enemy.TakeDamage(proj.Damage);
                proj.MarkForRemoval();
            }

            // collisions with player
            if (enemy.IsColliding(Player.GetInstance()))
            {
                player.TakeDamage(enemy.SlamDamage);
                var dir = Vector2.Normalize(Player.GetInstance().Position - enemy.Position) * 20f;
                player.ForcePush(dir);
                enemy.ForcePush(-dir);
            }
        }

        // update level
        if (Enemies.Count == 0) Level.StartNextWave(dt);
    }

    public static Vector2 ClampToScreen(Vector2 position, float size)
    {
        position.X = Math.Clamp(position.X, size, ScreenWidth - size);
        position.Y = Math.Clamp(position.Y, size, ScreenHeight - size);
        return position;
    }

    public static void SpawnProjectile(Projectile proj)
    {
        var projectileList = proj.FiredByPlayer ? ProjectilesFriendlyQueue : ProjectilesEnemyQueue;
        projectileList.Add(proj);
    }

    public static void SpawnEnemy(Entity enemy) => Enemies.Add(enemy);

    public static Vector2[] PointsAroundCircle(Vector2 center, float radius, int amount)
    {
        var points = new Vector2[amount];
        for (int i = 0; i < amount; i++)
        {
            var angle = 2 * MathF.PI * ((float)i / amount);
            points[i] = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        return points;
    }
}