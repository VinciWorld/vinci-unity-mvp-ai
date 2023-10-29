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

    private int dailyStepsIncrease = 100000;
    public int availableSteps = 0;

    public string _lastUpdatedDay = "2000-01-01";

    public event Action<int> stepsAvailableChange;


    public AgentBlueprint currentAgentConfig;
    public List<AgentBlueprint> agents = new List<AgentBlueprint>();


    //Relation between model and train env
    public Dictionary<string, TrainEnvironmentConfig> modelTrainingEnv = new Dictionary<string, TrainEnvironmentConfig>();
    

    public void AddAgent(AgentBlueprint newAgent)
    {
        currentAgentConfig = newAgent;
        agents.Add(newAgent);
    }

    public AgentBlueprint GetAgent(int index)
    {
        return agents[index];
    }

    public AgentBlueprint GetAgentById(string id)
    {
        return agents.FirstOrDefault(agent => agent.id == id);
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

    public void SubtractStepsAvailable(int steps)
    {
        availableSteps -= steps;
        stepsAvailableChange?.Invoke(availableSteps);
    }

    public void CheckAndIncreaseDailySteps()
    {
        DateTime today = DateTime.Now.Date;

        DateTime lastUpdatedDay = DateTime.Parse(_lastUpdatedDay);

        if (today > lastUpdatedDay)
        {
            availableSteps += dailyStepsIncrease;

            stepsAvailableChange?.Invoke(availableSteps);

            _lastUpdatedDay = today.ToString("yyyy-MM-dd");
        }
    }
}