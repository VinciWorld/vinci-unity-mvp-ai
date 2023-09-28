using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AgentConfig
{
    public string agentName;
    public string description;

    public GameObject AgentPrefab;
    public ModelConfig modelConfig;

    public string GetModelRunID()
    {
        return modelConfig.run_id;
    }

    public bool GetIsModelTrained()
    {
        return modelConfig.isModelTrained;
    }
}

