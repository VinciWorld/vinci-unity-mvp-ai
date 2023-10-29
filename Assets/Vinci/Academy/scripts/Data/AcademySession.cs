using System;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Academy.Environement;


[Serializable]
public class AcademySession
{
    public TrainEnvironmentConfig selectedTrainEnv;
    public AgentBlueprint selectedAgent;
    public GameObject currentAgentInstance;
    public EnvironementBase currentEnvInstance;


    public TrainJobStatus GetTrainJobStatus()
    {
        return selectedAgent.modelConfig.trainJobStatus;
    }

    public void SetIsModelLoaded(bool isTrained)
    {
        selectedAgent.SetIsModelLoaded(isTrained);
    }

    public void SetRunID(string runId)
    {
        selectedAgent.SetRunID(runId);
    }

    public string GetModelRunID()
    {
        return selectedAgent.GetModelRunID();
    }

    public bool GetIsModelLoaded()
    {
        return selectedAgent.GetIsModelLoaded();
    }


    public void AddOrUpdateEvaluationResults(string envId, Dictionary<string, string> evaluationResults)
    {
        selectedAgent.AddOrUpdateEvaluationResults(envId, evaluationResults);
    }

    public Dictionary<string, string> GetEvaluationResultsByKey(string envId)
    {
        return selectedAgent.GetEvaluationResultsByKey(envId);
    }
}