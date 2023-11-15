using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public class EnvHallway : EnvironementBase
{
    public GameObject ground;
    public GameObject area;
    public GameObject symbolOGoal;
    public GameObject symbolXGoal;
    public GameObject symbolO;
    public GameObject symbolX;
    
    [SerializeField]
    HallwaySettings hallwaySettings;

    Renderer m_GroundRenderer;
    Material _groundMaterial;

    HallwayAgent _agent;

    [Header("Replay")]
    private bool _isReplay = false;
    private int _episodeCount = 0;
    private Stack<ActionsHallwayMsg> actionStack = new Stack<ActionsHallwayMsg>();
    public float refreshRate = 0.02f; 
    private Coroutine replayActionsLoopCoroutine;
    private Pose _agentStartPose;
    private Pose _symbolOGoalPose;
    private Pose _symbolXGoalPose;
    private Pose _symbolOPose;
    private Pose _symbolXPose;

    private int goalsCompletedCount = 0;
    private int goalsFailedCount = 0;
    private float successRatio = 0f;

    public override event System.Action<string> actionsReceived;
    public override event System.Action<int, int, int> episodeCountStepCountTotalStepCountUpdated;
    public override event System.Action<Dictionary<string, MetricValue>> envMetricsUpdated;
    public override event System.Action<Dictionary<string, MetricValue>> commonMetricsUpdated;
    public override event System.Action<int> episodeCountUpdated;

    //public override event System.Action<Dictionary<string, MetricValue>> updateCommonResults;

    private bool isFirstEpisode = true;

    void Start()
    {
        hallwaySettings = GameObject.FindObjectOfType<HallwaySettings>();
    }

    public override void Initialize(GameObject agent)
    {
        _agent = agent.GetComponent<HallwayAgent>();
        _agent.env = this;
        _agent.hallwaySettings = hallwaySettings;

        m_GroundRenderer = ground.GetComponent<Renderer>();
        _groundMaterial = m_GroundRenderer.material;

        _agent.ResetPosition(ground.transform.position);

        goalsCompletedCount = 0;
        goalsFailedCount = 0;

        Debug.Log("Initialize env");
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
            var blockOffset = 0f;
            _agent.selection = Random.Range(0, 2);
            if (_agent.selection == 0)
            {
                symbolO.transform.position =
                    new Vector3(0f + Random.Range(-3f, 3f), 2f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
            }
            else
            {
                symbolO.transform.position =
                    new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
                symbolX.transform.position =
                    new Vector3(0f, 2f, blockOffset + Random.Range(-5f, 5f))
                    + ground.transform.position;
            }

            _agentStartPose = _agent.ResetPosition(ground.transform.position);

            var goalPos = Random.Range(0, 2);
            if (goalPos == 0)
            {
                symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }
            else
            {
                symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
                symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
            }

            _symbolOGoalPose = new Pose(symbolOGoal.transform.position, symbolOGoal.transform.rotation);
            _symbolXGoalPose = new Pose(symbolXGoal.transform.position, symbolXGoal.transform.rotation);
            _symbolOPose = new Pose(symbolO.transform.position, symbolO.transform.rotation);
            _symbolXPose = new Pose(symbolX.transform.position, symbolX.transform.rotation);
        }

        _episodeCount++;
    }


    public override void GoalCompleted(bool result)
    {        
        if (result)
        {
            goalsCompletedCount++;
            StartCoroutine(
                GoalScoredSwapGroundMaterial(hallwaySettings.goalScoredMaterial, 0.5f)
            );
        }
        else
        {
            goalsFailedCount++;
            StartCoroutine(
                GoalScoredSwapGroundMaterial(hallwaySettings.failMaterial, 0.5f)
            );
        }

        UpdateAndInvokeResults();
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = _groundMaterial;
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
        //updateCommonResults?.Invoke(metrics);
    }

    public override void Reset()
    {
        isFirstEpisode = false;
        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;
    }


    public override IAgent GetAgent()
    {
        return _agent;
    }
/*
    public override void SetAgentBehavior(BehaviorType type)
    {
        _agent.SetBehaviorType(type);
    }
*/
    public override void StartEnv(BehaviorType type, EnvMode mode)
    {
        Reset();
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
    public override void OnActionsFromServerReceived(string actionsJson)
    {
        ActionsHallwayMsg action = JsonConvert.DeserializeObject<ActionsHallwayMsg>(actionsJson);
        actionStack.Push(action);

        if (replayActionsLoopCoroutine == null)
        {
            replayActionsLoopCoroutine = StartCoroutine(ReplayActionsLoop());
        }

        Debug.Log("Received action from server: " + actionsJson);
    }

    public Queue<int> actionsQueueReceived;

    private IEnumerator ReplayActionsLoop()
    {
        int stepCount = 0;
        int totalStepCount = 0;
        Debug.Log("Start Replay");
        _agent.SetIsReplay(true);

        Time.timeScale = 10;
        while (_isReplay)
        {
            if (actionStack.Count > 0)
            {
                ActionsHallwayMsg action = actionStack.Pop();

                _agent.EndEpisode();
                // Prepare env
                _agent.selection = action.selection;
                _agent.transform.position = action.agentPose.GetPosition();
                _agent.transform.rotation = action.agentPose.GetRotation();
                symbolO.transform.position = action.symbolOPose.GetPosition();
                symbolX.transform.position = action.symbolXPose.GetPosition();
                symbolOGoal.transform.position = action.symbolOGoalPose.GetPosition();
                symbolXGoal.transform.position = action.symbolXGoalPose.GetPosition();

                actionsQueueReceived = new Queue<int>(action.actionsBuffer);
                _agent.actionsQueueReceived = actionsQueueReceived;

                Debug.Log("Actions received: " + actionsQueueReceived.Count);

                Academy.Instance.AutomaticSteppingEnabled = true;
                while(_agent.actionsQueueReceived.Count > 0)
                {
                    episodeCountStepCountTotalStepCountUpdated?.Invoke(action.episodeCount, _agent.steps, totalStepCount + _agent.steps);
                    yield return new WaitForEndOfFrame();
                }
                Academy.Instance.AutomaticSteppingEnabled = false;
            }
     
            /*
            foreach (int dir in action.actionsBuffer)
            {
                stepCount++;
                _agent.ActionFromServer(dir);

               

                episodeAndStepCountUpdated?.Invoke(action.episodeCount, totalStepCount + stepCount);
                Academy.Instance.EnvironmentStep();
                yield return new WaitForSeconds(refreshRate);
            }
            */
            totalStepCount += _agent.steps;
            _agent.steps = 0;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Leave replay");
    }

    public override void StopReplay()
    {
        _isReplay = false;
        _agent.SetIsReplay(false);
        StopCoroutine(replayActionsLoopCoroutine);
        replayActionsLoopCoroutine = null;
        Time.timeScale = 1f;
    }

    public override void SetIsReplay(bool isResplay)
    {
        _isReplay = isResplay;
    }

    public override void StartReplay()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricValue> GetEvaluaitonCommonTemplate()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricValue> GetEvaluaitonEnvTemplate()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricValue> GetEvaluationMetricCommonResults()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricValue> GetEvaluationMetricEnvResults()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricChange> GetEvaluationMetricEnvComparsionResults()
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<string, MetricChange> GetEvaluationMetricCommonComparsionResults()
    {
        throw new System.NotImplementedException();
    }

    public override void SetEvaluationEvents(System.Action<Dictionary<string, MetricValue>> commonMetricsUpdated, System.Action<Dictionary<string, MetricValue>> envMetricsUpdated, System.Action<Dictionary<string, MetricValue>> agentMetricsUpdated)
    {
        throw new System.NotImplementedException();
    }

    public override void RemoveListeners(System.Action<Dictionary<string, MetricValue>> commonMetricsUpdated, System.Action<Dictionary<string, MetricValue>> envMetricsUpdated, System.Action<Dictionary<string, MetricValue>> agentMetricsUpdated)
    {
        throw new System.NotImplementedException();
    }

    public override Dictionary<int, Dictionary<string, MetricValue>> GetEvaluationMetricAgentResults()
    {
        throw new System.NotImplementedException();
    }

    public override int episodeCount()
    {
        throw new System.NotImplementedException();
    }
}