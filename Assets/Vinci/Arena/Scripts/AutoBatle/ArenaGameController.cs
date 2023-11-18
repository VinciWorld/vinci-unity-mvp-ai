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

    private List<GenericAgent> agentsInGame = new List<GenericAgent>();

    private int _startingCoins = 100;
    public int currentCoins = 100;
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


    private bool isGameStarted;
    private int score;

    private const int POINTS_PER_WAVE = 1000;
    private const int POINTS_PER_KILL = 125;
    private const int COINS_PER_KILL = 10;

    public GameObject agentPrefab;


    void Awake()
     {
        _waveController = GetComponent<WaveController>();
        _placableEntityManager = GetComponent<PlacableEntityManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _placableEntityManager.Init(this);
        Initialize();

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        _gameHudView = ViewManager.GetView<GameHudView>();
        
        _placableEntityManager.agentDeployed += OnDeployedAgent;

        _waveController.completedWave += OnWaveCompleted;
        _waveController.completedAllWaves += OnAllWavesCompleted;

        _gameHudView.homeButtonPressed += OnHomeButtonPressed;
        _gameHudView.retryButtonPressed += OnRetryButtonPressed;
        _gameHudView.registerScoreOnBlockchainPressed += OnRegisterOnBlockchainButtonPressed;

        _gameHudView.upgradeDefenseButtonPressed += UpgradeDefense;
        _gameHudView.upgradeAttackButtonPessed += UpgradeAttack;
        _gameHudView.upgradeSpeedButtonPressed += UpgradeSpeed;

        Reset();

        _placableEntityManager.LoadAgentsCard();
    }



    void OnDeployedAgent(AgentBlueprint agentConfig, Vector3 position)
    {
        GameObject agent = Instantiate(
            agentPrefab, new Vector3(position.x, position.y + 0.1f, position.z)  , Quaternion.identity
        );

        GenericAgent waveAgent = agent.GetComponent<GenericAgent>();
        agentsInGame.Add(waveAgent);

        waveAgent.agentDied += OnAgentDied;
        waveAgent.agentKill += OnAgentKill;

        waveAgent.LoadModel(
            agentConfig.modelConfig.behavior.behavior_name, agentConfig.GetNNModel());

        waveAgent.SetBehaviorType(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);


        currentCoins -= 100;
        _gameHudView.UpdateCurrentCoins(currentCoins);

        if(!isGameStarted)
        {
            Academy.Instance.AutomaticSteppingEnabled = true;

            StartGame();
        }
    }

    public void StartGame()
    {
        isGameStarted = true;
        _waveController.StartWaves();
        Time.timeScale = 2;
    }

    private void OnRegisterOnBlockchainButtonPressed()
    {
        BlockchainManager.instance.RegisterPlayerScore(score);
    }

    private void OnRetryButtonPressed()
    {
        Reset();
        _placableEntityManager.LoadAgentsCard();
        //StartGame();
    }

    private void OnHomeButtonPressed()
    {
        Time.timeScale = 1;
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    public void OnAgentKill()
    {
        _agentsKills++;
        currentCoins += COINS_PER_KILL;
        _gameHudView.UpdateCurrentCoins(currentCoins);
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
        GameOver();
    }

    public void OnAgentDied(GenericAgent agent)
    {
        _agentDeaths++;
        _gameHudView.UpdateDeaths(_agentDeaths);
        agentsInGame.Remove(agent);

        if(agentsInGame.Count == 0)
        {
            GameOver();
        }
        else
        {
            Destroy(agent.GetGameObject());
        }
    }

    public void GameOver()
    {
        score = _currentWave * POINTS_PER_WAVE + _agentsKills * POINTS_PER_KILL;
        bool isHighScore = false;
        if(score > GameManager.instance.playerData.highScore)
        {
            isHighScore = true;
            GameManager.instance.playerData.highScore = score;
            GameManager.instance.SavePlayerData();
        }

        //Debug.Log("Deaths: " + _agentDeaths);

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
        isGameStarted = false;
        _gameHudView.SetInitialUpgradesCost(_defenseStatCost, _attackStatCost, _speedStatCost);
        _gameHudView.UpdateCurrentCoins(_startingCoins);
        _gameHudView.UpdateWavesCount(_currentWave);
        _gameHudView.UpdateKills(_agentsKills);
        _gameHudView.UpdateDeaths(_agentDeaths);
        _gameHudView.UpdateStats(100, 20, 50);

        _startingCoins = 100;
        currentCoins = 100;
        _defenseStatLevel = 0;
        _attackStatLevel = 0;
        _speedStatLevel = 0;
        _currentWave = 0;
        _agentsKills = 0;
        _agentDeaths = 0;

        _waveController.ResetWaves();

        foreach (var agemt in agentsInGame)
        {   
            if(agemt!= null)
            {
                Destroy(agemt.GetGameObject());
            }
        }

        _placableEntityManager.RemoveCards();
        Time.timeScale = 1;
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

        if (currentCoins >= _defenseStatCost)
        {
            currentCoins -= _defenseStatCost;
            _defenseStatLevel++;
            _gameHudView.UpdateCurrentCoins(currentCoins);

            Debug.Log("UPGRADE DEFENSE ");
        }
        else
        {
            _gameHudView.ShowNotEnoughCoins();
            Debug.Log("Unable to update DEFENSE");
        }
    }

    public void UpgradeAttack()
    {
        if (_attackStatLevel == MAX_LEVEL)
        {
            _gameHudView.MaxUpgradeReached();
            return;
        }

        if (currentCoins >= _attackStatCost)
        {
            currentCoins -= _attackStatCost;
            _attackStatLevel++;
            _gameHudView.UpdateCurrentCoins(currentCoins);
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

        if (currentCoins >= _speedStatCost)
        {
            currentCoins -= _speedStatCost;
            _speedStatLevel++;
            _gameHudView.UpdateCurrentCoins(currentCoins);
            Debug.Log("UPGRADE ATTACK");
        }
        else
        {
            _gameHudView.ShowNotEnoughCoins();
            Debug.Log("Unable to update ATTACK");
        }
    }
}
