using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Barracuda;
using System.Linq;
using Vinci.Academy.Environement;

[System.Serializable]
public class PlayerData
{
    public string username;

    public bool isPlayerRegisteredOnCompetition = false;
    public int highScore = 0;

    public bool modelFinishedTraining = false;

    public bool isLoggedIn = false;

    public AgentConfig currentAgentConfig;
    public List<AgentConfig> agents = new List<AgentConfig>();


    //Relation between model and train env
    public Dictionary<string, TrainEnvironmentConfig> modelTrainingEnv = new Dictionary<string, TrainEnvironmentConfig>();


    public void AddAgent(AgentConfig newAgent)
    {
        currentAgentConfig = newAgent;
        agents.Add(newAgent);
    }

    public AgentConfig GetAgent(int index)
    {
        return agents[index];
    }

    public void SetIsModelLoaded(bool isTrained)
    {
        currentAgentConfig.SetIsModelLoaded(isTrained);
    }

    public void SetRunID(string runId)
    {
        currentAgentConfig.SetRunID(runId);
    }

    public string GetModelRunID()
    {
        return currentAgentConfig.GetModelRunID();
    }

    public bool GetIsModelLoaded()
    {
        return currentAgentConfig.GetIsModelLoaded();
    }

    public void SetModelAndPath(string modelPath, NNModel nnModel)
    {
        SetModelAndPath(modelPath, nnModel);
    }

    public void AddOrUpdateEvaluationResults(string envId, Dictionary<string, string> evaluationResults)
    {
        currentAgentConfig.AddOrUpdateEvaluationResults(envId, evaluationResults);
    }

    public Dictionary<string, string> GetEvaluationResultsByKey(string envId)
    {
        return currentAgentConfig.GetEvaluationResultsByKey(envId);
    }
}