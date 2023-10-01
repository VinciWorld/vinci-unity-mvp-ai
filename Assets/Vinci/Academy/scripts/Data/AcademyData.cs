using UnityEngine;
using System;
using System.Collections.Generic;
using Vinci.Academy.Ml.Data;
using System.Linq;

[Serializable]
public class AcademyData
{
    public AcademySession session = new AcademySession();
    public List<AgentConfig> availableAgents;
    public List<TrainEnvironmentConfig> availableTrainEnvs;

    public TrainEnvironmentConfig GetTrainEnvById(string id)
    {
        return availableTrainEnvs.FirstOrDefault(env => env.id == id);
    }
}