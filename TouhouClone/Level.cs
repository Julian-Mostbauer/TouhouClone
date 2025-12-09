using System.Numerics;
using TouhouClone.Entities.Enemies;

namespace TouhouClone;

public class Level
{
    private readonly Wave[] _waves;
    private int _currentWaveIndex = 0;
    private Wave CurrentWave => _waves[_currentWaveIndex];
    public bool Completed => _currentWaveIndex >= _waves.Length;

    private Level(Wave[] waves) => _waves = waves;
    private float spawnTimer = 0f;
    private const float SpawnDelay = 1f;

    public static Level LoadFromFile(string filePath)
    {
        // TODO
        Wave[] waves =
        [
            new([
                EnemyFactory.CreateSimple(new Vector2(100, -100)),
                EnemyFactory.CreateSniper(new Vector2(700, -100))
            ]),
            new([
                EnemyFactory.CreateTank(new Vector2(400, -100)),
                EnemyFactory.CreateSniper(new Vector2(400, -100))
            ]),
            new Wave([
                EnemyFactory.CreateSimpleBoss(new Vector2(400, -100))
            ])
        ];
        return new Level(waves);
    }

    public void StartNextWave(float dt)
    {
        spawnTimer -= dt;
        if (Completed || spawnTimer > 0) return;
        foreach (var enemy in CurrentWave.Enemies) Game.SpawnEnemy(enemy);
        _currentWaveIndex++;
        spawnTimer = SpawnDelay;
    }

    public string GetCurrentWaveInfo() =>
        $"Wave {_currentWaveIndex} of {_waves.Length}";

    public string Serialize()
    {
        return string.Join("\n",
            _waves.Select(w =>
                string.Join(";", w.Enemies.Select(e => e.Serialize()))
            )
        );
    }
}

public record Wave(Entities.Enemy[] Enemies);