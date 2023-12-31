using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using Vinci.Academy.Environement;
using Vinci.Core.Managers;

[Serializable]
public class AgentConfig
{
    public string id;

    public string agentName;
    public string description;
    public Sprite agentImageFullBody;
    public Sprite agentImageHead;

    public int AgentPrice = 100;

    public List<TrainEnvironmentConfig> allowedEnvs;

    public GameObject AgentPrefab;
    
    public ModelConfig modelConfig;

    public NNModel GetNNModel()
    {
        //if(modelConfig.nnModel == null)
        //{
            return GameManager.instance.baseNNModel;
        //}

        //return modelConfig.nnModel;
    }

    public void AddTrainMetrics(float meanReward, float stdReward)
    {
        modelConfig.AddTrainMetrics(meanReward, stdReward);
    }

    public void AddStepsTrained(int stepsTrained)
    {
        modelConfig.AddStepsTrained(stepsTrained);
    }

    public void SetModelAndPath(string modelPath, NNModel nnModel)
    {
        modelConfig.nnModel_path = modelPath;
        modelConfig.nnModel = nnModel;

        modelConfig.isModelTraining = false;
        modelConfig.isModelLoaded = true;
    }

    public bool GetIsModelTraining()
    {
        return modelConfig.isModelTraining;
    }

    public void SetIsModelTraining(bool isTraining)
    {
        modelConfig.isModelTraining = isTraining;
    }

    public bool GetIsModelLoaded()
    {
        return modelConfig.isModelLoaded;
    }

    public void SetIsModelLoaded(bool isTrained)
    {
        modelConfig.isModelLoaded = isTrained;
    }

    public void SetRunID(string runId)
    {
        modelConfig.run_id = runId;
    }

    public string GetModelRunID()
    {
        return modelConfig.run_id;
    }

    public void AddOrUpdateEvaluationResults(string envId, Dictionary<string, string> evaluationResults)
    {
        modelConfig.isEvaluated = true;
        modelConfig.modelEnvsEvaluationsResults[envId] = evaluationResults;
    }

    public Dictionary<string, string> GetEvaluationResultsByKey(string envId)
    {
        modelConfig.modelEnvsEvaluationsResults.TryGetValue(envId, out Dictionary<string, string> results);
        return results; 
    }
}

