using System;
using UnityEngine;
using Vinci.Academy.Environement;


[Serializable]
public class AcademySession
{
    public TrainEnvironmentConfig selectedTrainEnv;
    public AgentConfig selectedAgent;
    public GameObject currentAgentInstance;
    public EnvironementBase currentEnvInstance;

}