using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;


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

    private bool _randomSpawn;
    private System.Random seededRandom;


    void Start()
    {
        int seed = 1987;
        seededRandom = new System.Random(seed);
    }

    public void Initialized(EnemyManager enemyManager, bool randomSpawn = false)
    {
        _enemyManager = enemyManager;
        _enemyManager.enemyKilled += OnEnemyDie;
        _randomSpawn = randomSpawn;
    }

    public void StartWaves()
    {
        NextWave();
    }

    private void Update()
    {   /*
        if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.intervalBetweenSpawns;

            _enemyManager.SpawnEnemy(_currentWave.enemyPrefab, GetRandomSpawnPoint());
        }
        */
    }

    private void NextWave()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex - 1 < waves.Length)
        {
            _currentWave = waves[_currentWaveIndex - 1];
            _enemiesRemainingToSpawn = _currentWave.amout;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;

            StartCoroutine(EnemySpawnerRoutine());
        }
        else
        {
            completedAllWaves?.Invoke();
        }
    }

    private IEnumerator EnemySpawnerRoutine()
    {
        while (_enemiesRemainingToSpawn > 0)
        {
            // Calculate the wait time
            float waitTime = _nextSpawnTime - Time.time;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            _enemiesRemainingToSpawn--;
            _enemyManager.SpawnEnemy(_currentWave.enemyPrefab, GetRandomSpawnPoint());
            _nextSpawnTime = Time.time + _currentWave.intervalBetweenSpawns;
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

    void OnDestroy()
    {
        _enemyManager.enemyKilled -= OnEnemyDie;
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
        int rnd = 0;
        if (_randomSpawn)
        {
             rnd = Random.Range(0, _spawnPoints.Count);
        }
        else
        {
            rnd = seededRandom.Next(0, _spawnPoints.Count);

        }

        return _spawnPoints[rnd].position;
    }
}