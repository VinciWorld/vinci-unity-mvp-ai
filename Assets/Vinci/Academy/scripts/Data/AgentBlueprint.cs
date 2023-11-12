using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Sentis;
using UnityEngine;
using Vinci.Academy.Environement;
using Vinci.Core.Managers;

[CreateAssetMenu(fileName = "Agent Blueprint", menuName = "Agent/AgentBlueprint", order = 1)]
public class AgentBlueprint : ScriptableObject
{
    public string id;

    public string agentName;
    public string description;
    public Sprite agentImageFullBody;
    public Sprite agentImageHead;

    public int AgentPrice = 100;

    public List<TrainEnvironmentConfig> allowedEnvs;

    public GameObject AgentPrefab;

    [JsonConverter(typeof(EnvMetricsDataConverter))]
    public ModelConfig modelConfig;

    public ModelAsset GetNNModel()
    {
        //if(modelConfig.nnModel == null)
        //{
            return GameManager.instance.baseNNModel;
        //}

        //return modelConfig.nnModel;
    }

    public void AddTrainMetrics(int stepCount, float meanReward, float stdReward, float timeElapsed)
    {
        modelConfig.AddTrainMetricsEntry(stepCount, meanReward, stdReward, timeElapsed);
    }

    public void AddStepsTrained(int stepsTrained)
    {
        modelConfig.AddStepsTrained(stepsTrained);
    }

    public void SetModelAndPath(string modelPath, ModelAsset nnModel)
    {
        modelConfig.nnModelResourcePath = modelPath;
        modelConfig.nnModel = nnModel;
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
        modelConfig.runId = runId;
    }

    public string GetModelRunID()
    {
        return modelConfig.runId;
    }

    public void AddOrUpdateCommonEvaluationMetrics(string envId, Dictionary<string, MetricValue> commonMetrics)
    {
        modelConfig.AddToCommonEvaluationMetrics(envId, commonMetrics);
    }

    public void StoreSessionEvaluationMetrics(
        string envId,
        Dictionary<string, MetricValue> commonMetrics,
        Dictionary<string, MetricValue> envMetrics,
        Dictionary<int, Dictionary<string, MetricValue>> agentMetrics
    )
    {
        modelConfig.StoreSessionEvaluationMetrics(envId, commonMetrics, envMetrics, agentMetrics);
    }

    public Dictionary<string, MetricValue> GetCommonEvaluationMetrics(string envId)
    {
        return modelConfig.GetCommonEvaluationMetrics(envId);
    }
    public void AddOrUpdateEnvEvaluationMetrics(string envId, Dictionary<string, MetricValue> envMetrics)
    {
        modelConfig.AddToEnvEvaluationMetrics(envId, envMetrics);
    }

    public Dictionary<string, MetricValue> GetEnvEvaluationMetrics(string envId)
    {
        return modelConfig.GetEnvEvaluationMetrics(envId);
    }
    public void AddOrUpdateAgentEvaluationMetrics(string envId, Dictionary<int, Dictionary<string, MetricValue>> agentMetrics)
    {
        modelConfig.AddToAgentEvaluationMetricsPerEpisode(envId, agentMetrics);
    }

    public Dictionary<int, Dictionary<string, MetricValue>> GetAgentEvaluationMetrics(string envId)
    {
        return modelConfig.GetAgentEvaluationMetricsPerEpisode(envId);
    }

}

