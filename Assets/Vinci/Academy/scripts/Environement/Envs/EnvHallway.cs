using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
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

    private int goalsCompletedCount = 0;
    private int goalsFailedCount = 0;
    private float successRatio = 0f;

    public override event System.Action<Dictionary<string, string>> updateEnvResults;

    void Start()
    {
        hallwaySettings = GameObject.FindObjectOfType<HallwaySettings>();
    }

    public override void Initialize(HallwayAgent agent)
    {
        _agent = agent;
        _agent.episodeBegin += EpisodeBegin;
        _agent.hallwaySettings = hallwaySettings;
        _agent.goalCompleted += GoalCompleted;

        m_GroundRenderer = ground.GetComponent<Renderer>();
        _groundMaterial = m_GroundRenderer.material;

        _agent.ResetPosition(ground.transform.position);

        goalsCompletedCount = 0;
        goalsFailedCount = 0;

    }

    public override void EpisodeBegin()
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

        _agent.ResetPosition(ground.transform.position);

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

    public override void GoalCompleted(bool result)
    {
        if(result)
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
}