using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvRobotWaves : EnvironementBase
{
    public int envId = 0;

    IAgent _agent;
    [SerializeField]
    WaveController _waveController;
    [SerializeField]
    EnemyManager _enemyManager;

    [SerializeField]
    Transform _agentSpawnPose;
    public Transform AgentSpawnPose => _agentSpawnPose;

    [SerializeField]
    ObservationHelper _observationHelper;

    [SerializeField]
    LoaderPopup _loaderPopup;

    public override event Action<Dictionary<string, MetricValue>> updateEnvResults;
    public override event Action<string> actionsReceived;
    public override event Action<int, int, int> episodeAndStepCountUpdated;

    [Header("Replay")]
    private bool _isReplay = false;
    private int _episodeCount = 0;
    public float refreshRate = 0.02f;
    private Coroutine replayActionsLoopCoroutine;
    private Stack<ActionsRobotWaveMsg> actionStack = new Stack<ActionsRobotWaveMsg>();
    public Queue<ActionRobotBufferMsg> actionsQueueReceived;

    private int goalsCompletedCount = 0;
    private int goalsFailedCount = 0;
    private float successRatio = 0f;

    int totalStepCount = 0;

    private bool isFirstEpisode = true;


    //Evaluation
    private EvaluationMetrics evaluationMetrics;
    Dictionary<string, MetricValue> envMetricsTemplate;

    public override void Initialize(GameObject agent)
    {
        _waveController.Initialized(_enemyManager);

        // _loaderPopup = GameObject.Find("PopupLoader").GetComponent<LoaderPopup>();
        _agent = agent.GetComponent<IAgent>();
        Debug.Log("RobotWaveAgentTrain" + _agent);

        _agent.SetEnv(this);
        _agent.SetObservationHelper(_observationHelper);

        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;
        totalStepCount = 0;

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

        GoalCompleted(true);
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
            _agent.SetAgentPose(_agentSpawnPose.position, _agentSpawnPose.rotation);
        }

        _episodeCount++;
    }

    public override IAgent GetAgent()
    {
        return _agent;
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

    public override void Reset()
    {
        _agent.Reset();
        _waveController.ResetWaves();
        _agent.SetAgentPose(_agentSpawnPose.position, _agentSpawnPose.rotation);
        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;
        _episodeCount = 0;
    }

    public override void SetAgentBehavior(BehaviorType type)
    {
        _agent.SetBehaviorType(type);
    }

    public override void StartEnv()
    {
        Reset();
        _agent.EndEpisode();
        Academy.Instance.AutomaticSteppingEnabled = true;
    }

    public override void StopEnv()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
    }

    //REPLAY
    //Server side code!
#if !UNITY_EDITOR && UNITY_SERVER
    private void SendEpisodeActionsToClient()
    {
        ActionsRobotWaveMsg actions = new ActionsRobotWaveMsg
        {
            stepCount = Academy.Instance.StepCount,
            episodeCount = _episodeCount,
            actionsBuffer = _agent.actionsBuffer,
            agentPose = new Pose(_agent.transform.position, _agent.transform.rotation)
        };

        
        string jsonActions = JsonConvert.SerializeObject(actions);
        actionsReceived?.Invoke(jsonActions);
        _agent.actionsBuffer.Clear();
    }
#endif


    public override void StartReplay()
    {
        _isReplay = true;
        Reset();
        //ShowLoaderPopup("Buffering episodes...");

    }
    // CLIENT SIDE
    public override void OnActionsFromServerReceived(string actions)
    {
        ActionsRobotWaveMsg action = JsonConvert.DeserializeObject<ActionsRobotWaveMsg>(actions);
        actionStack.Push(action);

        if (replayActionsLoopCoroutine == null)
        {
            replayActionsLoopCoroutine = StartCoroutine(ReplayActionsLoop());
        }

        //Debug.Log("Received action from server: " + actions);
    }
    bool isPopupUp;
    private IEnumerator ReplayActionsLoop()
    {
 
        Debug.Log("Start Replay");
        _agent.SetIsReplay(true);


        UpdateLoaderMessage("Buffering episode...");
        Time.timeScale = 2f;
        while (_isReplay)
        {

            if (actionStack.Count > 0)
            {
                ActionsRobotWaveMsg action = actionStack.Pop();

                _agent.EndEpisode();
                _agent.SetAgentPose(_agentSpawnPose.position, _agentSpawnPose.rotation);

                _waveController.ResetWaves();
                _waveController.StartWaves();


                actionsQueueReceived = new Queue<ActionRobotBufferMsg>(action.actionsBuffer);
                _agent.SetActionsQueueReceived(actionsQueueReceived);

                Debug.Log("Actions received: " + actionsQueueReceived.Count);

                Academy.Instance.AutomaticSteppingEnabled = true;

                CloseLoaderPopup();
                while (_agent.GetActionsQueueReceived().Count > 0)
                {
                    episodeAndStepCountUpdated?.Invoke(action.episodeCount, _agent.GetSteps(), _agent.GetSteps() + totalStepCount);
                    yield return new WaitForEndOfFrame();
                }
                episodeAndStepCountUpdated?.Invoke(action.episodeCount, _agent.GetSteps(), _agent.GetSteps() + totalStepCount);
                Academy.Instance.AutomaticSteppingEnabled = false;

                _waveController.ResetWaves();
                _agent.SetAgentPose(_agentSpawnPose.position, _agentSpawnPose.rotation);

                totalStepCount += _agent.GetSteps();
                _agent.SetSteps(0);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public override void SetIsReplay(bool isResplay)
    {
        _isReplay = isResplay;
    }

    public override void StopReplay()
    {
        Debug.Log("Stop reaply: " + replayActionsLoopCoroutine);
        _isReplay = false;
        _agent.SetIsReplay(false);
        if(replayActionsLoopCoroutine != null)
        {
            StopCoroutine(replayActionsLoopCoroutine);
        }
        replayActionsLoopCoroutine = null;

        Time.timeScale = 1f;
    }

    public void ShowLoaderPopup(string messange)
    {
        _loaderPopup.gameObject.SetActive(true);
        _loaderPopup.SetProcessingMEssage(messange);
        _loaderPopup.Open();
    }

    public void UpdateLoaderMessage(string messange)
    {
        _loaderPopup.SetProcessingMEssage(messange);
    }

    public void CloseLoaderPopup()
    {
        _loaderPopup.Close();
    }

    #region Evaluations Metrics

    private void InitializeEvaluationMetrics()
    {
        evaluationMetrics = new EvaluationMetrics();

        envMetricsTemplate = new Dictionary<string, MetricValue>
        {
            {"Avg kills", new MetricValue(MetricType.Int, 0)},
            {"Avg shoots missed", new MetricValue(MetricType.Float, 0.0)},
            {"Avg shoots hits", new MetricValue(MetricType.Float, 0.0f)}
        };
        evaluationMetrics.Initialize(envId.ToString(), envMetricsTemplate);
    }

    public override Dictionary<string, MetricValue> GetEvaluaitonCommonTemplate()
    {
        return evaluationMetrics.GetZeroedCommonTemplate(envId.ToString());
    }

    public override Dictionary<string, MetricValue> GetEvaluaitonEnvTemplate()
    {
        return evaluationMetrics.GetZeroedEnvTemplate(envId.ToString());
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

    private void UpdateEvaluationCommonResults()
    {
        // Calculate success ratio
        float totalGoals = goalsCompletedCount + goalsFailedCount;
        successRatio = totalGoals > 0 ? (float)goalsCompletedCount / totalGoals : 0;

        // Create and populate the results dictionary
        Dictionary<string, MetricValue> metrics = new Dictionary<string, MetricValue>
        {
            { "Goal Completed", new MetricValue(MetricType.Int, goalsCompletedCount) },
            { "Goal Failed",  new MetricValue(MetricType.Int, goalsFailedCount)  },
            { "Goal Success Ratio",  new MetricValue(MetricType.Percent, successRatio)  } // P2 formats the number as a percentage
        };

        // Invoke the event with the results dictionary
        updateEnvResults?.Invoke(metrics);
    }

    public override Dictionary<string, string> GetEvaluationMetricTeamplate()
    {
        throw new NotImplementedException();
    }

    #endregion

}