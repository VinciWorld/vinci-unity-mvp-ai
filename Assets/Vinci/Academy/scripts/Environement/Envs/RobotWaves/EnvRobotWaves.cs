using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvRobotWaves : EnvironementBase
{
    RobotWaveAgent _agent;

    [SerializeField]
    WaveController _waveController;

    [SerializeField]
    Transform _agentSpawnPose;
    public Transform AgentSpawnPose => _agentSpawnPose;

    public override event Action<Dictionary<string, string>> updateEnvResults;
    public override event Action<string> actionsReceived;
    public override event Action<int, int> episodeAndStepCountUpdated;

    [Header("Replay")]
    private bool _isReplay = false;
    public float refreshRate = 0.02f;
    private Coroutine replayActionsLoopCoroutine;
    private Stack<ActionsRobotWaveMsg> actionStack = new Stack<ActionsRobotWaveMsg>();
    public Queue<ActionRobotBufferMsg> actionsQueueReceived;

    private int goalsCompletedCount = 0;
    private int goalsFailedCount = 0;
    private float successRatio = 0f;

    public override void Initialize(GameObject agent)
    {
        _agent = agent.GetComponent<RobotWaveAgent>();
        _agent.env = this;

        Debug.Log("Initilize: " + _agent.env);

        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;

        _waveController.completedAllWaves += OnCompletedAllWaves;
        _waveController.completedWave += OnCompletedWave;
    }

    public void OnCompletedWave()
    {
        Debug.Log("COMPLETE WAVE");
        //robotAgent.AddReward(0.05f);
    }

    public void OnCompletedAllWaves()
    {
        Debug.Log("Complete all Waves");
        _agent.AddReward(1);
        _agent.EndEpisode();
    }


    public override void EpisodeBegin()
    {
#if !UNITY_EDITOR && UNITY_SERVER
        if(!isFirstEpisode)
        {
            SendEpisodeActionsToClient();
        }
        else
        {
            isFirstEpisode = false;
        }
#endif

        if (!_isReplay)
        {
            _agent.Reset();
            _waveController.ResetWaves();
            _waveController.StartWaves();
            _agent.transform.position = _agentSpawnPose.position;
            _agent.transform.rotation = _agentSpawnPose.rotation;
        }
    }

    public override IAgent GetAgent()
    {
        return _agent;
    }

    public override Dictionary<string, string> GetEvaluationMetricResults()
    {
        Dictionary<string, string> metrics = new Dictionary<string, string>
        {
            { "Goal Completed", goalsCompletedCount.ToString() },
            { "Goal Failed", goalsFailedCount.ToString() },
            { "Goal Success Ratio", successRatio.ToString("P2") } // P2 formats the number as a percentage
        };

        return metrics;
    }

    public override void GoalCompleted(bool result)
    {
        if(result)
        {
            goalsCompletedCount++;
        }
        else
        {
            goalsFailedCount++;
        }

        UpdateAndInvokeResults();
    }

    private void UpdateAndInvokeResults()
    {
        // Calculate success ratio
        float totalGoals = goalsCompletedCount + goalsFailedCount;
        successRatio = totalGoals > 0 ? (float)goalsCompletedCount / totalGoals : 0;

        // Create and populate the results dictionary
        Dictionary<string, string> metrics = new Dictionary<string, string>
        {
            { "Goal Completed", goalsCompletedCount.ToString() },
            { "Goal Failed", goalsFailedCount.ToString() },
            { "Goal Success Ratio", successRatio.ToString("P2") } // P2 formats the number as a percentage
        };

        // Invoke the event with the results dictionary
        updateEnvResults?.Invoke(metrics);
    }

    public override void Reset()
    {
        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;
    }

    public override void SetAgentBehavior(BehaviorType type)
    {
        _agent.SetBehaviorType(type);
    }

    public override void StartEnv()
    {
        Reset();
        Academy.Instance.AutomaticSteppingEnabled = true;
    }

    public override void StopEnv()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
    }

    public override void SetIsReplay(bool isResplay)
    {
        _isReplay = isResplay;
    }

    public override void StopReplay()
    {
        _isReplay = false;
        _agent.isReplay = false;
        StopCoroutine(replayActionsLoopCoroutine);
        replayActionsLoopCoroutine = null;
        Time.timeScale = 1f;
    }

    //REPLAY
    //Server side code!
#if !UNITY_EDITOR && UNITY_SERVER
    private void SendEpisodeActionsToClient()
    {
        ActionsHallwayMsg actions = new ActionsHallwayMsg
        {
            stepCount = Academy.Instance.StepCount,
            episodeCount = _episodeCount,
            actionsBuffer = _agent.actionsBuffer,

            selection = _agent.selection,
            agentPose = _agentStartPose,
            symbolOGoalPose = _symbolOGoalPose,
            symbolXGoalPose = _symbolXGoalPose,
            symbolOPose = _symbolOPose,
            symbolXPose = _symbolXPose
        };

        
        string jsonActions = JsonConvert.SerializeObject(actions);
        actionsReceived?.Invoke(jsonActions);
        _agent.actionsBuffer.Clear();
    }
#endif

    // CLIENT SIDE
    public override void OnActionsFromServerReceived(string actions)
    {
        ActionsRobotWaveMsg action = JsonConvert.DeserializeObject<ActionsRobotWaveMsg>(actions);
        actionStack.Push(action);

        if (replayActionsLoopCoroutine == null)
        {
            replayActionsLoopCoroutine = StartCoroutine(ReplayActionsLoop());
        }

        Debug.Log("Received action from server: " + actions);
    }

    private IEnumerator ReplayActionsLoop()
    {
        int totalStepCount = 0;
        Debug.Log("Start Replay");
        _agent.isReplay = true;

        Time.timeScale = 10;
        while (_isReplay)
        {
            if (actionStack.Count > 0)
            {
                ActionsRobotWaveMsg action = actionStack.Pop();

                _agent.EndEpisode();
                _agent.transform.position = action.agentPose.GetPosition();
         
                actionsQueueReceived = new Queue<ActionRobotBufferMsg>(action.actionsBuffer);
                _agent.actionsQueueReceived = actionsQueueReceived;

                Debug.Log("Actions received: " + actionsQueueReceived.Count);

                Academy.Instance.AutomaticSteppingEnabled = true;
                while (_agent.actionsQueueReceived.Count > 0)
                {
                    episodeAndStepCountUpdated?.Invoke(action.episodeCount, totalStepCount + _agent.steps);
                    yield return new WaitForEndOfFrame();
                }
                Academy.Instance.AutomaticSteppingEnabled = false;
            }

            totalStepCount += _agent.steps;
            _agent.steps = 0;
            yield return new WaitForEndOfFrame();
        }
    }

}