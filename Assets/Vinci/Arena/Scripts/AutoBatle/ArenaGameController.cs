using System.Collections;
using System.Collections.Generic;
using StatSystem;
using Unity.MLAgents;
using UnityEngine;

public class ArenaGameController : MonoBehaviour
{
    WaveController _waveController;
    PlacableEntityManager _placableEntityManager;

    List<RobotWaveAgent> agentsInGame = new List<RobotWaveAgent>();

    private int startingCoins = 100;
    private int currentCoins = 100;

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
        _placableEntityManager.LoadAgentsCard();
        _placableEntityManager.agentDeployed += OnDeployedAgent;

        currentCoins = startingCoins;
    }

    void OnDeployedAgent(AgentConfig agentConfig, Vector3 position)
    {
        GameObject agent = Instantiate(
            agentConfig.AgentPrefab, position, Quaternion.identity
        );

        RobotWaveAgent waveAgent = agent.GetComponent<RobotWaveAgent>();
        agentsInGame.Add(waveAgent);

        waveAgent.agentDied += OnAgentDied;

        waveAgent.LoadModel(
            agentConfig.modelConfig.behavior.behavior_name, agentConfig.GetNNModel());

        waveAgent.SetBehaviorType(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);

        Academy.Instance.AutomaticSteppingEnabled = true;

        StartGame();
    }

    public void StartGame()
    {
        _waveController.StartWaves();
    }

    public void OnAgentDied(RobotWaveAgent agent)
    {
        agentsInGame.Remove(agent);

        if(agentsInGame.Count == 0)
        {
            Debug.Log("GameOver");
        }

        Destroy(agent.gameObject);
    }

    private void OnDisable() 
    {   
        foreach (var agent in agentsInGame)
        {
            agent.agentDied -= OnAgentDied;
        }

        _placableEntityManager.agentDeployed -= OnDeployedAgent;
    }

    public void UpgradeStat(StatType statType)
    {
        if(statType == StatType.HEALTH)
        {
            int cost = 75;

            if(currentCoins >= cost)
            {
                Debug.Log("UPGRADE HEALTH ");
            }
            else
            {
                Debug.Log("Unable to update HEALTH");
            }
            
        }
        else if(statType == StatType.SPEED)
        {
            int cost = 100;

            if (currentCoins >= cost)
            {
                Debug.Log("UPGRADE SPEED");
            }
            else
            {
                Debug.Log("Unable to update SPEED");
            }
        }
        else if(statType == StatType.ATTACK)
        {
            int cost = 75;

            if (currentCoins >= cost)
            {
                Debug.Log("UPGRADE ATTACK");
            }
            else
            {
                Debug.Log("Unable to update ATTACK");
            }
        }
    }
}
