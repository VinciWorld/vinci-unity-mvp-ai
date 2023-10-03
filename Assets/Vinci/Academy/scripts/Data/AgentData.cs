using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using Vinci.Academy.Environement;

[Serializable]
public class AgentConfig
{
    public string agentName;
    public string description;

    public List<TrainEnvironmentConfig> allowedEnvs;

    public GameObject AgentPrefab;
    
    public ModelConfig modelConfig;



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

