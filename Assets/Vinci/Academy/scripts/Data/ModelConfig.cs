using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
//using MLAgents;

[Serializable]
public class ModelConfig
{
    public string run_id;

    public BehaviorConfigSmall behavior;

    public TrainJobStatus trainJobStatus;

    public bool isModelSubmitted = false;
    public bool isModelTraining = false;
    public bool isModelSucceeded = false;

    public bool isModelLoaded = false;
    public bool isModelMinted = false;
    public bool modelFinishedTraining = false;

    public bool isEvaluated = false;

    public string nnModel_path;

    [System.NonSerialized]
    public NNModel nnModel;

    public ModelTrainMetrics trainMetrics = new();
    public Dictionary<string, Dictionary<string, string>> modelEnvsEvaluationsResults = new();

    public void AddTrainMetrics(float meanReward, float stdReward)
    {
        trainMetrics.meanReward = meanReward;
        trainMetrics.stdReward = stdReward;
    }

    public void AddStepsTrained(int stepsTrained)
    {
        trainMetrics.stepsTrained = stepsTrained;
        trainMetrics.totalStepsTrained += stepsTrained;
    }
}

[Serializable]
public class ModelTrainMetrics
{
    public float meanReward = 0.0f;
    public float stdReward = 0.0f;
    public int stepsTrained = 0;
    public int totalStepsTrained = 0;
}
