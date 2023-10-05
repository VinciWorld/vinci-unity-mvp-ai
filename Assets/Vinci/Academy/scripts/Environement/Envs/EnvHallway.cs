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
    public bool isReplay = false;
    private int _episodeCount = 0;
    private Queue<ActionsHallwayMsg> actionQueue = new Queue<ActionsHallwayMsg>();
    public float refreshRate = 0.1f; 
    private Coroutine replayActionsLoopCoroutine;
    private Pose _agentStartPose;
    private Pose _symbolOGoalPose;
    private Pose _symbolXGoalPose;
    private Pose _symbolOPose;
    private Pose _symbolXPose;

    private int goalsCompletedCount = 0;
    private int goalsFailedCount = 0;
    private float successRatio = 0f;

    public override event System.Action<Dictionary<string, string>> updateEnvResults;
    public override event System.Action<string> actionsReceived;
    public override event System.Action<int, int> episodeAndStepCountUpdated;

    void Start()
    {
        hallwaySettings = GameObject.FindObjectOfType<HallwaySettings>();
    }

    public override void Initialize(HallwayAgent agent)
    {
        _agent = agent;
        _agent.env = this;
        _agent.hallwaySettings = hallwaySettings;

        m_GroundRenderer = ground.GetComponent<Renderer>();
        _groundMaterial = m_GroundRenderer.material;

        _agent.ResetPosition(ground.transform.position);

        goalsCompletedCount = 0;
        goalsFailedCount = 0;

    }

    public override void EpisodeBegin()
    {
        _episodeCount++;

        if(!isReplay)
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
        }

        _symbolOGoalPose = new Pose(symbolOGoal.transform.position, symbolOGoal.transform.rotation);
        _symbolXGoalPose = new Pose(symbolXGoal.transform.position, symbolXGoal.transform.rotation);
        _symbolOPose = new Pose(symbolO.transform.position, symbolO.transform.rotation);
        _symbolXPose = new Pose(symbolX.transform.position, symbolX.transform.rotation);

    }


    public override void GoalCompleted(bool result)
    {

#if !UNITY_EDITOR && UNITY_SERVER
        SendEpisodeActionsToClient();
#endif
        
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
        updateEnvResults?.Invoke(metrics);
    }

    public override void Reset()
    {
        goalsCompletedCount = 0;
        goalsFailedCount = 0;
        successRatio = 0;
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

    public override HallwayAgent GetAgent()
    {
        return _agent;
    }

    public override void SetAgentBehavior(BehaviorType type)
    {
        _agent.SetBehaviorType(type);
    }

    public override void StartEnv()
    {
        Reset();
        Debug.Log("Enable!");
        Academy.Instance.AutomaticSteppingEnabled = true;
    }

    public override void StopEnv()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
    }


    //REPLAY
    //Server side code!
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

        _agent.actionsBuffer.Clear();
        string jsonActions = JsonConvert.SerializeObject(actions);
        actionsReceived?.Invoke(jsonActions);
    }

    // CLIENT SIDE
    public override void OnActionsFromServerReceived(string actionsJson)
    {
        ActionsHallwayMsg action = JsonConvert.DeserializeObject<ActionsHallwayMsg>(actionsJson);
        actionQueue.Enqueue(action);

        if (replayActionsLoopCoroutine == null)
        {
            replayActionsLoopCoroutine = StartCoroutine(ReplayActionsLoop());
        }

        Debug.Log("Received action from server: " + actionsJson);
    }

    private IEnumerator ReplayActionsLoop()
    {
        int stepCount = 0;
        while (actionQueue.Count > 0)
        {
            ActionsHallwayMsg action = actionQueue.Dequeue();

            // Prepare env
            _agent.selection = action.selection;
            symbolO.transform.position = action.symbolOPose.GetPosition();
            symbolX.transform.position = action.symbolXPose.GetPosition();
            symbolOGoal.transform.position = action.symbolOGoalPose.GetPosition();
            symbolXGoal.transform.position = action.symbolXGoalPose.GetPosition();

            foreach (int dir in action.actionsBuffer)
            {
                stepCount++;
                _agent.ActionFromServer(dir);

                episodeAndStepCountUpdated?.Invoke(action.episodeCount, stepCount);
                Academy.Instance.EnvironmentStep();
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }
}