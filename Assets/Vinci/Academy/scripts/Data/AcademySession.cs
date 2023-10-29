using System;
using UnityEngine;
using Vinci.Academy.Environement;


[Serializable]
public class AcademySession
{
    public TrainEnvironmentConfig selectedTrainEnv;
    public AgentBlueprint selectedAgent;
    public GameObject currentAgentInstance;
    public EnvironementBase currentEnvInstance;

}