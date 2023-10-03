using System;
using UnityEngine;

[Serializable]
public class AgentConfig
{
    public string agentName;
    public string description;

    [System.NonSerialized]
    public GameObject AgentPrefab;
    
    public ModelConfig modelConfig;



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

}

