using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string username;

    public AgentConfig currentAgentConfig;

    public List<AgentConfig> agents;

    public void AddAgent(AgentConfig newAgent)
    {
        currentAgentConfig = newAgent;
        agents.Add(newAgent);
    }

    public AgentConfig GetAgent(int index)
    {
        return agents[index];
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
}