using System;
using UnityEngine;
//using MLAgents;

[Serializable]
public class ModelConfig
{
    public string run_id;
    public bool isModelTraining = false;
    public bool isModelLoaded = false;

    public BehaviorConfig behavior;
    public String nnModel_path;

    //public NNModel nnModel;

}