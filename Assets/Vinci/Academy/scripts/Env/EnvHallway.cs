using System.Collections;
using Unity.MLAgents;
using UnityEngine;

public class EnvHallway : MonoBehaviour
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

    void Start()
    {
        hallwaySettings = GameObject.FindObjectOfType<HallwaySettings>();
    }

    public void Initialize(HallwayAgent agent)
    {
        _agent = agent;
        _agent.episodeBegin += OnEpisodeBegin;
        _agent.hallwaySettings = hallwaySettings;
        _agent.goalCompleted += OnGoalComplete;

        m_GroundRenderer = ground.GetComponent<Renderer>();
        _groundMaterial = m_GroundRenderer.material;
    }

    void OnEpisodeBegin()
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

    public void OnGoalComplete(bool result)
    {
        if(result)
        {
            StartCoroutine(
                GoalScoredSwapGroundMaterial(hallwaySettings.goalScoredMaterial, 0.5f)
            );
        }
        else
        {
            StartCoroutine(
                GoalScoredSwapGroundMaterial(hallwaySettings.failMaterial, 0.5f)
            );
        }
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = _groundMaterial;
    }
}