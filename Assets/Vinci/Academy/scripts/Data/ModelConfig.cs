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

    public bool isModelTraining = false;
    public bool isModelLoaded = false;
    public bool isModelMinted = false;

    public bool isEvaluated = false;

    public string nnModel_path;

    [System.NonSerialized]
    public NNModel nnModel;

    public ModelPerformance performance;

    public Dictionary<string, Dictionary<string, string>> modelEnvsEvaluationsResults = new();

}

[Serializable]
public class ModelPerformance
{
    public float meanReward = 0.0f;
    public float stdReward = 0.0f;
    public int stepsTrained = 0;
}
