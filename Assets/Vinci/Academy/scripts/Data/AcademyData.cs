using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Vinci.Academy.Environement;

[Serializable]
public class AcademyData
{
    public List<AgentConfig> availableAgents;
    public List<TrainEnvironmentConfig> availableTrainEnvs;

    public TrainEnvironmentConfig GetTrainEnvById(string id)
    {
        return availableTrainEnvs.FirstOrDefault(env => env.env_id == id);
    }

    public AgentConfig GetAgentById(string id)
    {
        return availableAgents.FirstOrDefault(agent => agent.id == id);
    }
}