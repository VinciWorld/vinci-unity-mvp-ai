using UnityEngine;
using System;
using System.Collections.Generic;
using Vinci.Academy.Ml.Data;

[Serializable]
public class AcademyData
{
    public AcademySession session = new AcademySession();
    public List<AgentConfig> availableAgents;
    public List<TrainEnvironment> availableTrainEnvs;
}