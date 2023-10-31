using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public abstract class EnvironementBase : MonoBehaviour
{
    //public abstract event Action<Dictionary<string, MetricValue>> updateCommonResults;
    public abstract event Action<string> actionsReceived;
    public abstract event Action<int, int, int> episodeAndStepCountUpdated;
    public abstract event Action<Dictionary<string, MetricValue>> envMetricsUpdated;
    public abstract event Action<Dictionary<string, MetricValue>> commonMetricsUpdated;

    public abstract void StartReplay();
    public abstract void SetIsReplay(bool isResplay);
    public abstract void StopReplay();

    public abstract IAgent GetAgent();

    public abstract void Initialize(GameObject agent);
    public abstract void EpisodeBegin();
    public abstract void GoalCompleted(bool result);
    public abstract void Reset();

    public abstract void StartEnv();
    public abstract void StopEnv();

    public abstract void SetAgentBehavior(BehaviorType type);

    //Evaluation
    public abstract void SetEvaluationEvents(
        Action<Dictionary<string, MetricValue>> commonMetricsUpdated,
        Action<Dictionary<string, MetricValue>> envMetricsUpdated,
        Action<Dictionary<string, MetricValue>> agentMetricsUpdated
    );

    public abstract void RemoveListeners(
        Action<Dictionary<string, MetricValue>> commonMetricsUpdated,
        Action<Dictionary<string, MetricValue>> envMetricsUpdated,
        Action<Dictionary<string, MetricValue>> agentMetricsUpdated
    );

    public abstract Dictionary<string, MetricValue> GetEvaluaitonCommonTemplate();
    public abstract Dictionary<string, MetricValue> GetEvaluaitonEnvTemplate();
    public abstract Dictionary<string, MetricValue> GetEvaluationMetricCommonResults();
    public abstract Dictionary<string, MetricValue> GetEvaluationMetricEnvResults();
    public abstract Dictionary<string, MetricChange> GetEvaluationMetricEnvComparsionResults();
    public abstract Dictionary<string, MetricChange> GetEvaluationMetricCommonComparsionResults();

    public abstract void OnActionsFromServerReceived(string actions);
}