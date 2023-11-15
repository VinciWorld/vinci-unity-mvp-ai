using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public enum EnvMode
{
    NONE,
    TRAIN,
    REPLAY,
    EVAUATION
}


public abstract class EnvironementBase : MonoBehaviour
{
    protected EnvMode currentEnvMode = EnvMode.NONE;

    //public abstract event Action<Dictionary<string, MetricValue>> updateCommonResults;
    public abstract event Action<string> actionsReceived;
    public abstract event Action<int, int, int> episodeCountStepCountTotalStepCountUpdated;
    public abstract event Action<Dictionary<string, MetricValue>> envMetricsUpdated;
    public abstract event Action<Dictionary<string, MetricValue>> commonMetricsUpdated;
    public abstract event Action<int> episodeCountUpdated;

    public abstract int episodeCount();


    public abstract void StartEnv(BehaviorType behaviorType, EnvMode mode);
    public abstract void StopEnv();

    public abstract void StartReplay();
    public abstract void SetIsReplay(bool isResplay);
    public abstract void StopReplay();

    public abstract IAgent GetAgent();

    public abstract void Initialize(GameObject agent);
    public abstract void EpisodeBegin();
    public abstract void GoalCompleted(bool result);
    public abstract void Reset();

    //public abstract void SetAgentBehavior(BehaviorType type);

    //Evaluation
    public abstract void SetEvaluationEvents(
        Action<Dictionary<string, MetricValue>> commonMetricsUpdated,
        Action<Dictionary<string, MetricValue>> agentMetricsUpdated,
        Action<Dictionary<string, MetricValue>> envMetricsUpdated = null
    );

    public abstract void RemoveListeners(
        Action<Dictionary<string, MetricValue>> commonMetricsUpdated,
        Action<Dictionary<string, MetricValue>> agentMetricsUpdated,
        Action<Dictionary<string, MetricValue>> envMetricsUpdated = null
    );

    public abstract Dictionary<string, MetricValue> GetEvaluaitonCommonTemplate();
    public abstract Dictionary<string, MetricValue> GetEvaluaitonEnvTemplate();
    public abstract Dictionary<string, MetricValue> GetEvaluationMetricCommonResults();
    public abstract Dictionary<string, MetricValue> GetEvaluationMetricEnvResults();
    public abstract Dictionary<string, MetricChange> GetEvaluationMetricEnvComparsionResults();
    public abstract Dictionary<string, MetricChange> GetEvaluationMetricCommonComparsionResults();
    public abstract Dictionary<int, Dictionary<string, MetricValue>> GetEvaluationMetricAgentResults();

    public abstract void OnActionsFromServerReceived(string actions);
}