using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using UnityEngine;
using Vinci.Academy.Environement;
using Vinci.Core.StateMachine;

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
        config.env_id = "1";
        config.num_of_areas = 6;
        config.agent_id = "1";

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
            envConfig, trainEnvConfig.num_of_areas, 100f
        );

       for(int i = 0; i < envsInstances.Count; i++)
       {
            if(i == 0) mainEnv = envsInstances[i];

            GameObject created_agent = AgentFactory.instance.CreateAgent(
                agentConfig,
                new Vector3(0, 1.54f, -8.5f), Quaternion.identity,
                envsInstances[i].transform

            );

            envsInstances[i].Initialize(created_agent);
        }

        //mainEnv.GetAgent().episodeBegin += OnEpisodeBegin;
      
        mainEnv.actionsReceived += OnActionReceived;
        mainEnv.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.Default);
        Academy.Instance.AutomaticSteppingEnabled = true;
        RemoteTrainManager.instance.SendWebSocketJson("Instance: Start Training");
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

    //Send action to unity client
    void OnActionReceived(string actionsJson)
    {
        //Debug.Log("Action sent: " + actionsJson);
        RemoteTrainManager.instance.SendWebSocketJson(actionsJson);
    }

    void OnTrainJobConfigReceived(PostResponseTrainJob trainJob)
    {
        Debug.Log("Config received: " + trainJob.env_config);

        StartTraining(trainJob.env_config);
    }
}