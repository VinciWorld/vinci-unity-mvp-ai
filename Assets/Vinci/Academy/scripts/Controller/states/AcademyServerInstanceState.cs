using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Barracuda;
using Unity.MLAgents;
using UnityEngine;
using Vinci.Academy.Environement;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyServerInstanceState : StateBase
{
    AcademyController _controller;
    AcademyTrainView trainView;

    EnvironementBase mainEnv;

    public AcademyServerInstanceState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {

        Academy.Instance.AutomaticSteppingEnabled = false;

        EnvConfigSmall config = new EnvConfigSmall();
        config.env_id = "0001";
        config.num_of_areas = 8;
        config.agent_id = "999";

        //StartTraining(config);

        ConnectWebSocketToTrainInstance();
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    public void StartTraining(EnvConfigSmall trainEnvConfig)
    {
        Debug.Log("START TRAINNING");

        TrainEnvironmentConfig envConfig = _controller.academyData.GetTrainEnvById(trainEnvConfig.env_id);
        AgentConfig agentConfig = _controller.academyData.GetAgentById(trainEnvConfig.agent_id);

        if(envConfig == null)
        {
            Debug.LogError("Unable to find env with id: " + trainEnvConfig.env_id);
        }

        if (agentConfig == null)
        {
            Debug.LogError("Unable to find agent with id: " + trainEnvConfig.agent_id);
        }

        List<EnvironementBase> envsInstances = _controller.envManager.CreateMutipleTrainEnvs(
            envConfig, trainEnvConfig.num_of_areas, 5f
        );

       for(int i = 0; i < envsInstances.Count; i++)
       {
            if(i == 0) mainEnv = envsInstances[i];

            GameObject created_agent = AgentFactory.instance.CreateAgent(
                agentConfig,
                new Vector3(0, 1.54f, -8.5f), Quaternion.identity,
                envsInstances[i].transform

            );

            envsInstances[i].Initialize(created_agent.GetComponent<HallwayAgent>());
        }

        RemoteTrainManager.instance.SendWebSocketJson("Instance: Start Training");

        Academy.Instance.AutomaticSteppingEnabled = true;
    }

    void ConnectWebSocketToTrainInstance()
    {
        RemoteTrainManager.instance.trainJobConfigReceived += OnTrainJobConfigReceived;
        try
        {
            RemoteTrainManager.instance.ConnectWebSocketToTrainInstance();
        }
        catch(Exception e)
        {
            Debug.LogError("Unable to connect Websocket train instante: " + e.Message);
        }
    }

    void OnEpisodeBegin()
    {
        EpisodeBeginMsg episodeBeginMsg = new EpisodeBeginMsg();
        string json = JsonConvert.SerializeObject(episodeBeginMsg);

        RemoteTrainManager.instance.SendWebSocketJson(json);
    }

    void OnActionReceived(string actionsJson)
    {
        //Debug.Log("Action received: " + actionsJson);
        RemoteTrainManager.instance.SendWebSocketJson(actionsJson);
    }

    void OnTrainJobConfigReceived(PostResponseTrainJob trainJob)
    {
        Debug.Log("Config received: " + trainJob.env_config);

        StartTraining(trainJob.env_config);
    }
}