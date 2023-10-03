using System;
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

    public string nnModel_path;
    [System.NonSerialized]
    public NNModel nnModel;
}