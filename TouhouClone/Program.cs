using System;
using System.Numerics;
using Raylib_cs;

namespace TouhouClone
{
    internal static class Program
    {
        private static readonly Random Random = new();
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 600;
        private static readonly List<Projectile> ProjectilesFriendly = new(50);
        private static readonly List<Projectile> ProjectilesEnemy = new(100);
        private static readonly List<Entity> Enemies = new(100);

        private static readonly Vector2 ScreenCenter = new(ScreenWidth / 2f, ScreenHeight / 2f);

        private static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Touhou Clone - Raylib template");
            Raylib.SetTargetFPS(60);

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
                    enemy.TakeDamage(10);
                    proj.MarkForRemoval();
                }

                if (enemy.IsColliding(Player.GetInstance()))
                {
                    player.TakeDamage(10);
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

        private static Vector2 ClampToScreen(Vector2 position, float size)
        {
            position.X = Math.Clamp(position.X, size, ScreenWidth - size);
            position.Y = Math.Clamp(position.Y, size, ScreenHeight - size);
            return position;
        }

        private static void SpawnProjectile(Vector2 position, Vector2 velocity, bool firedByPlayer)
        {
            var projectileList = firedByPlayer ? ProjectilesFriendly : ProjectilesEnemy;
            var proj = new Projectile
            {
                Position = position,
                Velocity = velocity,
                Size = 5f,
                Color = firedByPlayer ? Color.Green : Color.Red
            };
            projectileList.Add(proj);
        }

        private static void SpawnEnemy(Vector2 position, BehaviorModel behaviorModel)
        {
            Enemies.Add(new SimpleEntity(position, behaviorModel));
        }

        internal abstract class GameObj
        {
            public Vector2 Position = Vector2.Zero;
            public float Size;
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

            public bool fullyOnScreen()
            {
                return Position.X + Size >= 0 && Position.X - Size <= ScreenWidth &&
                       Position.Y + Size >= 0 && Position.Y - Size <= ScreenHeight;
            }

            public void ForcePush(Vector2 force) => _forceVelocity += force;

            public void MarkForRemoval() => IsActive = false;
        }

        private class Projectile : GameObj
        {
            public Vector2 Velocity;
            public Color Color;

            public override void Update(float dt)
            {
                if (!fullyOnScreen())
                {
                    MarkForRemoval();
                    return;
                }

                Position += Velocity * dt;
            }

            public override void Draw()
            {
                Raylib.DrawCircleV(Position, Size, Color);
            }
        }

        private abstract class Entity : GameObj
        {
            protected int Health;
            protected int MaxHealth;
            public int GetHealth => Health;
            public int GetMaxHealth => MaxHealth;
            public bool IsAlive => Health > 0;

            public virtual void TakeDamage(int amount) => Health -= amount;
        }

        private record BehaviorModel(
            float TargetChange,
            float SpeedChange,
            float ShootChance,
            float PlayerBias,
            float CenterBias)
        {
            public static readonly BehaviorModel Default = new(0.02f, 0.01f, 0.02f, -100f, 100f);
            public static readonly BehaviorModel Tackler = new(0.02f, 0.01f, 0.0f, 500f, 100f);
        };

        private class SimpleEntity : Entity
        {
            private float _speed = 100f;
            private Vector2 _goal = new(0, 0);
            private static readonly Color[] Colors = [Color.Yellow, Color.Orange, Color.Red];
            private Color Color => Colors[CalculateColorIndex(Health, MaxHealth)];
            private readonly BehaviorModel _model;

            public SimpleEntity(Vector2 position, BehaviorModel model)
            {
                Position = position;
                _model = model;
                Size = 20f;
                MaxHealth = 250;
                Health = 250;
            }

            private static int CalculateColorIndex(int health, int maxHealth)
            {
                float healthRatio = (float)health / maxHealth;
                int index = (int)((1 - healthRatio) * Colors.Length);
                return Math.Clamp(index, 0, Colors.Length - 1);
            }

            public override void Update(float dt)
            {
                if (!IsAlive)
                {
                    MarkForRemoval();
                    return;
                }

                base.Update(dt);

                Movement(dt);
                Shooting();
                Position = ClampToScreen(Position, Size);
            }

            private void Shooting()
            {
                if (!(Random.NextDouble() < _model.ShootChance)) return;
                var directionToPlayer =
                    Vector2.Normalize(Player.GetInstance().Position - Position);
                SpawnProjectile(Position, directionToPlayer * 200f, false);
            }

            private void Movement(float dt)
            {
                // Choose a new random goal occasionally
                if (_goal.Length() == 0 || Random.NextDouble() < _model.TargetChange)
                {
                    float x = (float)(Random.NextDouble() * ScreenWidth);
                    float y = (float)(Random.NextDouble() * ScreenHeight);
                    var point = new Vector2(x, y);
                    var center = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
                    var toCenter = Vector2.Normalize(center - point);
                    var toPlayer = Vector2.Normalize(Player.GetInstance().Position - point);
                    // bias away from player, towards center
                    var bias = toCenter * _model.CenterBias + toPlayer * _model.PlayerBias;
                    _goal = point + bias;
                }

                // adjust speed based on randomness and health (less health = faster)
                if (Random.NextDouble() < _model.SpeedChange)
                    _speed += (float)(Random.NextDouble() - ((float)Health / MaxHealth)) * 20f;
                _speed = Math.Clamp(_speed, 50f, 200f);

                // calculate velocity towards goal
                var directionToGoal = Vector2.Normalize(_goal - Position);

                var vel = directionToGoal * _speed;
                // Move and bounce off-screen edges
                Position += vel * dt;
                if (!fullyOnScreen()) vel = -vel;
            }

            public override void Draw() => Raylib.DrawCircleV(Position, Size, Color);
        }

        private class Player : Entity
        {
            public Vector2 Velocity;
            private static Player? _instance;
            public readonly float Speed;
            private readonly float _shootCooldown = 0.2f;
            private float _remainingShootCooldown = 0f;
            private const float InvulnerabilityTime = .1f;
            private float _remainingInvulnerability = 0f;
            

            public static Player GetInstance() => _instance ??= new Player();

            private Player()
            {
                Size = 24f;
                Speed = 2500f;
                MaxHealth = 100;
                Velocity = Vector2.Zero;
                Position = ScreenCenter;
                Health = MaxHealth;
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                if (Velocity.Length() <= 0.1f) Velocity = Vector2.Zero;
                Position += Velocity * dt;
                Velocity /= (1 + dt * 5);
                Position = ClampToScreen(Position, Size);
                if (_remainingInvulnerability > 0)
                    _remainingInvulnerability -= dt;
                if (_remainingShootCooldown > 0)
                    _remainingShootCooldown -= dt;
            }

            public override void Draw()
            {
                Raylib.DrawCircleV(Position, Size, Color.Blue);
                Raylib.DrawCircleV(Position, Size / 2, Color.White);
            }
            
            private void MakeInvulnerable()
            {
                _remainingInvulnerability = InvulnerabilityTime;
            }

            private bool IsInvulnerable() => _remainingInvulnerability > 0;
            public override void TakeDamage(int amount)
            {
                if (IsInvulnerable()) return;
                base.TakeDamage(amount);
                MakeInvulnerable();
            }

            public void Shoot()
            {
                if (_remainingShootCooldown > 0) return;
                SpawnProjectile(Position, new Vector2(0, -500f), true);
                _remainingShootCooldown = _shootCooldown;
            }

            public override string ToString() =>
                $"Player(Health: {Health}/{MaxHealth}, Position: {Position}, Velocity: {Velocity}, Speed: {Speed}, Size: {Size}, ForceVelocity: {Velocity})";
        }
    }
}