using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class WaveController : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _spawnPoints = new List<Transform>();
    public List<Transform> SpawnPoints => _spawnPoints;

    public Wave[] waves;

    [SerializeField]
    EnemyManager _enemyManager;

    private Wave _currentWave;
    private int _currentWaveIndex;

    private int _enemiesRemainingToSpawn;
    private int _enemiesRemainingAlive;
    private float _nextSpawnTime;

    public event Action completedWave;
    public event Action completedAllWaves;


    public void StartWaves()
    {
        NextWave();
    }

    private void Update()
    {
        if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.intervalBetweenSpawns;

            _enemyManager.SpawnEnemy(_currentWave.enemyPrefab, GetRandomSpawnPoint(), OnEnemyDie);
        }
    }

    private void NextWave()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex - 1 < waves.Length)
        {
            _currentWave = waves[_currentWaveIndex - 1];
            _enemiesRemainingToSpawn = _currentWave.amout;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;
        }
        else
        {
            completedAllWaves?.Invoke();
        }
    }

    private void OnEnemyDie(EnemyAI enemyAI)
    {
        _enemiesRemainingAlive--;
        enemyAI.died -= OnEnemyDie;

        Destroy(enemyAI.gameObject);

        if (_enemiesRemainingAlive == 0)
        {
            completedWave?.Invoke();
            NextWave();
        }
    }

    public void ResetWaves()
    {
        _currentWaveIndex = 0;
        _enemiesRemainingToSpawn = 0;
        _enemiesRemainingAlive = 0;
        _nextSpawnTime = 0;
        _enemyManager.ResetEnemiesAi();
    }


    public Vector3 GetRandomSpawnPoint()
    {
        int rnd = Random.Range(0, _spawnPoints.Count);
        return _spawnPoints[rnd].position;
    }
}