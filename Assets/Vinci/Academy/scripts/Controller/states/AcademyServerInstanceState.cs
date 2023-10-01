using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEngine;
using Vinci.Academy.Ml.Data;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyServerInstanceState : StateBase
{
    AcademyController _controller;
    AcademyTrainView trainView;

    EnvHallway mainEnv;

    public AcademyServerInstanceState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        ConnectWebSocketToTrainInstance();
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    public void StartTraining(string envId)
    {
        TrainEnvironmentConfig envConfig = _controller.academyData.GetTrainEnvById(envId);
        List<GameObject> envsInstances = _controller.envManager.CreateMutipleTrainEnvs(envConfig, 6);

       for(int i = 0; i < envsInstances.Count; i++)
       {

            EnvHallway env = envsInstances[i].GetComponent<EnvHallway>();

            if(i == 0) mainEnv = env;

            GameObject created_agent = AgentFactory.instance.CreateAgent(
                _controller.manager.playerData.GetAgent(0),
                new Vector3(0, 1.54f, -8.5f), Quaternion.identity

            );

            env.Initialize(created_agent.GetComponent<HallwayAgent>());
        }

        mainEnv.agent.episodeBegin += OnEpisodeBegin;
        mainEnv.agent.actionsReceived += OnActionReceived;
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
        RemoteTrainManager.instance.SendWebSocketJson("true");
    }

    void OnActionReceived(string actionsJson)
    {
        RemoteTrainManager.instance.SendWebSocketJson(actionsJson);
    }

    void OnTrainJobConfigReceived(PostResponseTrainJob trainJob)
    {
        Debug.Log("Config received: " + trainJob.env_config);

        StartTraining(trainJob.env_config);
    }
}