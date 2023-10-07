using System;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvRobotWaves : EnvironementBase
{
    public override event Action<Dictionary<string, string>> updateEnvResults;
    public override event Action<string> actionsReceived;
    public override event Action<int, int> episodeAndStepCountUpdated;

    public override void EpisodeBegin()
    {
        throw new NotImplementedException();
    }

    public override HallwayAgent GetAgent()
    {
        throw new NotImplementedException();
    }

    public override Dictionary<string, string> GetEvaluationMetricResults()
    {
        throw new NotImplementedException();
    }

    public override void GoalCompleted(bool result)
    {
        throw new NotImplementedException();
    }

    public override void Initialize(HallwayAgent agent)
    {
        throw new NotImplementedException();
    }

    public override void OnActionsFromServerReceived(string actions)
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetAgentBehavior(BehaviorType type)
    {
        throw new NotImplementedException();
    }

    public override void SetIsReplay(bool isResplay)
    {
        throw new NotImplementedException();
    }

    public override void StartEnv()
    {
        throw new NotImplementedException();
    }

    public override void StopEnv()
    {
        throw new NotImplementedException();
    }

    public override void StopReplay()
    {
        throw new NotImplementedException();
    }
}