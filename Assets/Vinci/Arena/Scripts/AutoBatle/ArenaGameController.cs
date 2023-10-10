using System;
using System.Collections;
using System.Collections.Generic;
using StatSystem;
using Unity.MLAgents;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class ArenaGameController : MonoBehaviour
{
    private GameHudView _gameHudView;
    private WaveController _waveController;
    private PlacableEntityManager _placableEntityManager;

    private List<RobotWaveAgent> agentsInGame = new List<RobotWaveAgent>();

    private int _startingCoins = 100;
    private int _currentCoins = 100;
    private int _currentWave = 0;

    private int _agentsKills = 0;
    private int _agentDeaths = 0;

    private const int MAX_LEVEL = 10;
    private const int _defenseStatCost = 75;
    private int _defenseStatLevel = 0;
    private const int _attackStatCost = 75;
    private int _attackStatLevel = 0;
    private const int _speedStatCost = 150;
    private int _speedStatLevel = 0;

    private int score;

    private const int POINTS_PER_WAVE = 1000;
    private const int POINTS_PER_KIL = 125;


    void Awake()
     {
        _waveController = GetComponent<WaveController>();
        _placableEntityManager = GetComponent<PlacableEntityManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        _gameHudView = ViewManager.GetView<GameHudView>();
        _placableEntityManager.LoadAgentsCard();
        _placableEntityManager.agentDeployed += OnDeployedAgent;

        _waveController.completedWave += OnWaveCompleted;
        _waveController.completedAllWaves += OnAllWavesCompleted;

        _gameHudView.retryButtonPressed += OnRetryButtonPressed;
        _gameHudView.registerScoreOnBlockchainPressed += OnRegisterOnBlockchainButtonPressed;

        _gameHudView.upgradeDefenseButtonPressed += UpgradeDefense;
        _gameHudView.upgradeAttackButtonPessed += UpgradeAttack;
        _gameHudView.upgradeSpeedButtonPressed += UpgradeSpeed;
    }



    void OnDeployedAgent(AgentConfig agentConfig, Vector3 position)
    {
        GameObject agent = Instantiate(
            agentConfig.AgentPrefab, position, Quaternion.identity
        );

        RobotWaveAgent waveAgent = agent.GetComponent<RobotWaveAgent>();
        agentsInGame.Add(waveAgent);

        waveAgent.agentDied += OnAgentDied;
        waveAgent.agentKill += OnAgentKill;

        waveAgent.LoadModel(
            agentConfig.modelConfig.behavior.behavior_name, agentConfig.GetNNModel());

        waveAgent.SetBehaviorType(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);

        Academy.Instance.AutomaticSteppingEnabled = true;

        StartGame();
    }

    public void StartGame()
    {
        _currentCoins = _startingCoins;

        _gameHudView.SetInitialUpgradesCost(_defenseStatCost, _attackStatCost, _speedStatCost);
        _gameHudView.UpdateCurrentCoins(_startingCoins);
        _gameHudView.UpdateWavesCount(_currentWave);
        _gameHudView.UpdateKills(_agentsKills);
        _gameHudView.UpdateDeaths(_agentDeaths);
        _gameHudView.UpdateStats(100, 20, 50);

        _waveController.StartWaves();
    }

    private void OnRegisterOnBlockchainButtonPressed()
    {
        BlockchainManager.instance.RegisterCompetitionScore(score);
    }

    private void OnRetryButtonPressed()
    {
        StartGame();
    }


    public void OnAgentKill()
    {
        _agentsKills++;
        _gameHudView.UpdateKills(_agentsKills);
    }

    public void OnWaveCompleted()
    {
        _currentWave++;
        _gameHudView.UpdateWavesCount(_currentWave);
    }

    public void OnAllWavesCompleted()
    {
        Debug.Log("All waves completed");

        //_gameHudView.ShowGameOver(
        //    _currentWave, 
        //)
        GameOver();
    }

    public void OnAgentDied(RobotWaveAgent agent)
    {
        _agentDeaths++;
        _gameHudView.UpdateDeaths(_agentDeaths);
        agentsInGame.Remove(agent);

        if(agentsInGame.Count == 0)
        {
            GameOver();
        }

        Destroy(agent.gameObject);
    }

    public void GameOver()
    {
        score = _currentWave * POINTS_PER_WAVE + _agentsKills * POINTS_PER_KIL;
        bool isHighScore = false;
        if(score > GameManager.instance.playerData.highScore)
        {
            isHighScore = true;
            GameManager.instance.playerData.highScore = score;
            GameManager.instance.SavePlayerData();
        }

        _gameHudView.ShowGameOver(
            _currentWave,
            _agentsKills,
            _agentDeaths,
            isHighScore,
            score

        );
        Reset();
    }

    public void Reset()
    {
        _waveController.ResetWaves();

        foreach (var agemt in agentsInGame)
        {
            Destroy(agemt);
        }

        _startingCoins = 100;
        _currentCoins = 100;
        _defenseStatLevel = 0;
        _attackStatLevel = 0;
        _speedStatLevel = 0;
        _currentWave = 0;
        _agentsKills = 0;
        _agentDeaths = 0;
    }

    private void OnDisable() 
    {   
        foreach (var agent in agentsInGame)
        {
            agent.agentDied -= OnAgentDied;
        }

        _placableEntityManager.agentDeployed -= OnDeployedAgent;
    }

    //UPGRADES

    public void UpgradeDefense()
    {
        if (_defenseStatLevel == MAX_LEVEL)
        {
            _gameHudView.MaxUpgradeReached();
            return;
        }

        if (_currentCoins >= _defenseStatCost)
        {
            _currentCoins -= _defenseStatCost;
            _defenseStatLevel++;
            _gameHudView.UpdateCurrentCoins(_currentCoins);

            Debug.Log("UPGRADE HEALTH ");
        }
        else
        {
            _gameHudView.ShowNotEnoughCoins();
            Debug.Log("Unable to update HEALTH");
        }
    }

    public void UpgradeAttack()
    {
        if (_attackStatLevel == MAX_LEVEL)
        {
            _gameHudView.MaxUpgradeReached();
            return;
        }

        if (_currentCoins >= _attackStatCost)
        {
            _currentCoins -= _attackStatCost;
            _attackStatLevel++;
            _gameHudView.UpdateCurrentCoins(_currentCoins);
            Debug.Log("UPGRADE SPEED");
        }
        else
        {
            _gameHudView.ShowNotEnoughCoins();
            Debug.Log("Unable to update SPEED");
        }
    }

    public void UpgradeSpeed()
    {   
        if(_speedStatLevel == MAX_LEVEL)
        {
            _gameHudView.MaxUpgradeReached();
            return;
        }

        if (_currentCoins >= _speedStatCost)
        {
            _currentCoins -= _speedStatCost;
            _speedStatLevel++;
            _gameHudView.UpdateCurrentCoins(_currentCoins);
            Debug.Log("UPGRADE ATTACK");
        }
        else
        {
            _gameHudView.ShowNotEnoughCoins();
            Debug.Log("Unable to update ATTACK");
        }
    }
}
