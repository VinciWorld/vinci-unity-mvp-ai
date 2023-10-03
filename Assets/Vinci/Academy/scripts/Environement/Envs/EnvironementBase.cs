using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public abstract class EnvironementBase : MonoBehaviour
{
    public abstract event Action<Dictionary<string, string>> updateEnvResults;

    public abstract HallwayAgent GetAgent();

    public abstract void Initialize(HallwayAgent agent);
    public abstract void EpisodeBegin();
    public abstract void GoalCompleted(bool result);
    public abstract void Reset();

    public abstract void StartEnv();
    public abstract void StopEnv();

    public abstract void SetAgentBehavior(BehaviorType type);

    public abstract Dictionary<string, string> GetEvaluationMetricResults();
}