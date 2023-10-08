using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public enum SpawnMethod
{
    waves,
    survival,
}


public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private EnemyAI _enemyPrefab;

    [SerializeField]
    SpawnMethod _spawnMethod;


    private IObjectPool<EnemyAI> _enemyPool;

    private List<EnemyAI> _enemies = new List<EnemyAI>();
    private List<Transform> _spawnPoints = new List<Transform>();

    private int[] _lastSpawnUsed;
    private int indexLasSpawn = 1;

    public event Action KilledAllEnemies;
    public event Action<int> enemyKilled;

    private Transform _target;

    //STATS
    private int _killedEnemies = 0;
    public int KilledEnemies => _killedEnemies;
    private int _totalSpawns = 0;

    //CONFIG
    public int _numOfStartingEnemies = 4;
    //Increase the amount of enemies in game each time the player killed this amount;
    public int increaseSpawnAmoutbyKills = 10;
    public int numerOfEnemiesIncreased = 1;

    public int _maxToSpawn = 2000;
    public int MaxToSpawn => _maxToSpawn;


    public void Initialize(List<Transform> spawnPoints)
    {
        if (_enemyPool == null)
        {
            _enemyPool = new ObjectPool<EnemyAI>(CreateEnemy, OnGetEnemy, OnReleaseEnemy, OnDestroyEnemy);
        }

        _spawnPoints = spawnPoints;
        _lastSpawnUsed = new int[_spawnPoints.Count - 1];
    }

    public void StartSpawningEnemies()
    {
        _totalSpawns = 0;
        _killedEnemies = 0;
        numerOfEnemiesIncreased = 1;


        for (int i = 0; i < _numOfStartingEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnWave(Wave wave)
    {

    }

    void OnEnemyDie(EnemyAI enemy)
    {
        if (this.gameObject.activeSelf)
        {
            _enemyPool.Release(enemy);
            _enemies.Remove(enemy);
        }


        if (_spawnMethod == SpawnMethod.survival)
        {
            if (_totalSpawns < _maxToSpawn)
            {
                SpawnEnemy();
            }

            if (_killedEnemies != 0 && _killedEnemies % increaseSpawnAmoutbyKills == 0)
            {
                for (int i = 0; i < numerOfEnemiesIncreased; i++)
                {
                    SpawnEnemy();
                }
            }
        }
        else if (_spawnMethod == SpawnMethod.survival)
        {
        }

        _killedEnemies++;
        enemyKilled?.Invoke(_killedEnemies);

        if (_killedEnemies == _maxToSpawn - 1)
        {

        }
        // Debug.Log("killed: " + _killedEnemies + "amout of enemies: " + _enemies.Count);
    }

    private void SpawnEnemy()
    {
        EnemyAI enemy = _enemyPool.Get();

        enemy.gameObject.SetActive(true);
        enemy.Reset();

        Vector3 spawnPos = GetValidSpawnPos();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
        {
            enemy.SetPosition(hit.position);
        }

        //enemy.setTarget(_target);
        enemy.died += OnEnemyDie;
        _enemies.Add(enemy);

        _totalSpawns++;
    }

    List<EnemyAI> allEnemies = new();
    public void SpawnEnemy(EnemyAI enenyPrefab, Vector3 spawnPos, Action<EnemyAI> OnDie)
    {
        EnemyAI enemy = Instantiate(enenyPrefab, spawnPos, Quaternion.identity);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
        {
            enemy.SetPosition(hit.position);
        }

        allEnemies.Add(enemy);


        enemy.died += OnDie;
        enemy.died += OnEnemyAiDied;
    }

    public void OnEnemyAiDied(EnemyAI enemyAI)
    {
        enemyAI.died -= OnEnemyAiDied;
        allEnemies.Remove(enemyAI);
    }

    public void ResetEnemiesAi()
    {
        foreach (var enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }

        allEnemies.Clear();
    }

    private EnemyAI CreateEnemy()
    {
        //TODO: In the future change the spawn position, it may give problems
        EnemyAI enemy = Instantiate(_enemyPrefab, Vector3.zero, Quaternion.identity);
        return enemy;
    }

    private void OnGetEnemy(EnemyAI enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseEnemy(EnemyAI enemy)
    {
        enemy.died -= OnEnemyDie;
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(EnemyAI enemy)
    {
        Destroy(enemy.gameObject);
    }

    public void Reset()
    {
        indexLasSpawn = 1;
        _totalSpawns = 0;
        _killedEnemies = 0;

        foreach (EnemyAI enemy in _enemies)
        {
            if (enemy.gameObject.activeSelf)
                _enemyPool.Release(enemy);
        }

        _enemies.Clear();
    }

    Vector3 GetValidSpawnPos()
    {
        int index = UnityEngine.Random.Range(0, _spawnPoints.Count);
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            index = UnityEngine.Random.Range(0, _spawnPoints.Count);
            bool isEqual = false;
            for (int j = 0; j < _lastSpawnUsed.Length; j++)
            {
                if (_lastSpawnUsed[j] == index)
                {
                    isEqual = true;
                    break;
                }
            }
            if (!isEqual) break;
        }
        _lastSpawnUsed[indexLasSpawn++ % _lastSpawnUsed.Length] = index;

        Transform randomTransform = _spawnPoints[index].transform;

        return randomTransform.position;
    }
}