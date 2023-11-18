using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Vinci.Core.BattleFramework;

public class RobottWaveAgent : GenericAgent
{

    [SerializeField]
    private RobotWave _robot;
    InputControllerBasic _input;

    //Sensor
    RadiiusSensor _radiusSensor;
    DebugTextureDrawer debugTextureDrawer;

    public override event Action<GenericAgent> agentDied;
    public override event Action agentKill;

    //Metrics
    private int killsPerEpisode;
    private int shootHitsPerEpisode;
    private int shootsMissedPerEpisode;


    //Energy
    private const float startingEnergy = 50;
    private const float maxEnergy = 100f;  // Max energy the robot can have
    private float currentEnergy = startingEnergy; // The robot starts with max energy
    private const float energyRestorationRate = 1f; // Amount of energy restored per second
    private const float energyRequiredToShoot = 2; // Energy required to shoot
    float threshold = 10;

    protected override void Awake()
    {
        base.Awake();
        _robot = GetComponent<RobotWave>();
        _input = GetComponent<InputControllerBasic>();
        _radiusSensor = GetComponent<RadiiusSensor>();
    }

    void Start()
    {
        threshold = 0.25f * maxEnergy;

        _robot.targetable.hitTarget += OnhitTarget;
        _robot.targetable.missedTarget += OnMissedTarget;
        _robot.targetable.killedTarget += OnKilledTarget;
        _robot.targetable.died += OnDied;
    }

    private void FixedUpdate()
    {
        RestoreEnergy(Time.fixedDeltaTime);
    }

    protected override void InitAgent(Vector3 envOrigin)
    {
        _radiusSensor.mapOrigin = envOrigin;
        Debug.Log(agentId);
        Debug.Log(envOrigin);

        if (agentId == 0)
        {
            debugTextureDrawer = FindObjectOfType<DebugTextureDrawer>();

            debugTextureDrawer.sensor = _radiusSensor;
        }
    }

    protected override void OnAgentEpisodeBegin()
    {
        killsPerEpisode = 0;
        shootHitsPerEpisode = 0;
        shootsMissedPerEpisode = 0;
    }

    protected override void AgentCollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(currentEnergy / 100);
        sensor.AddObservation(_robot.CanShoot() && currentEnergy >= energyRequiredToShoot);
/*
        bool isLokking = false;
        if (environamentSensor != null)
        {

            if (agentId == 0)
                Debug.Log("isLooking: " + isLokking);
            isLokking = environamentSensor.IsPlayerLookingAtClosestEnemy(transform);
        }
*/

        Vector2 agentPosNorm = _radiusSensor.NormalizePosition(new Vector2(transform.position.x, transform.position.z));
        sensor.AddObservation(agentPosNorm);
        sensor.AddObservation(new Vector2(transform.forward.x, transform.forward.z));
        //sensor.AddObservation(isLokking);

        _radiusSensor.AddObservationsToSensor(sensor);
    }

    protected override void OnAgentActionReceived(ActionBuffers actionBuffers) 
    {
        var discreteActionsOut = actionBuffers.DiscreteActions;

        if (currentEnergy < threshold)
        {
            float ratio = 1 - (currentEnergy / maxEnergy);

            float negativeReward = -0.001f * ratio;

            AddReward(negativeReward);
        }
        else
        {
            AddReward(0.001f);
        }

        var action = discreteActionsOut[0];

        _robot.MoveAgent(action);

        if (discreteActionsOut[1] == 1)
        {
            if(currentEnergy >= energyRequiredToShoot &&_robot.Shoot())
            {
                currentEnergy -= energyRequiredToShoot;
            }
        }

        //AddReward(-1f / MaxStep);
    }

    protected override void AgentHeuristic(in ActionBuffers actionsOut) 
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        _robot.HeuristicSimple(discreteActionsOut);
    }

    private void RestoreEnergy(float deltaTime)
    {
        currentEnergy = Mathf.Min(currentEnergy + energyRestorationRate * deltaTime, maxEnergy);
    }

    private void OnDied(DamageableObject @object, float arg2, Vector3 vector)
    {
        //Debug.Log("DIEDDDDD");
        AddReward(-1f);
        agentDied?.Invoke(this);

        if (_env != null)
        {
            _env.GoalCompleted(false);
        }

        EndEpisode();
    }

    private void OnGUI() 
    {
        if(agentId == 0)
        {

            // Initialize the GUIStyle
            GUIStyle labelStyle = new GUIStyle();

            // Set the font size
            labelStyle.fontSize = 36; // Adjust the size as needed

            // Optional: Set other font attributes
            labelStyle.normal.textColor = Color.white;
            // Define the position and size of the label
            Rect labelRect = new Rect(10, 10, 100, 20); // (x, y, width, height)

            // Draw the label with the number
            GUI.Label(labelRect, currentEnergy.ToString(), labelStyle);
        }
    }

#region Metrics
    private void OnKilledTarget()
    {
        killsPerEpisode++;
        RegisterMetric( MetricKeys.Kills.ToString(), MetricType.Int, killsPerEpisode);
        
        AddReward(0.05f);

        agentKill?.Invoke();
    }

    private void OnMissedTarget()
    {
        //Debug.Log("miss ");
        shootsMissedPerEpisode++;
        RegisterMetric(MetricKeys.Shoots_Missed.ToString(), MetricType.Int, shootsMissedPerEpisode);

        AddReward(-0.05f);
    }

    private void OnhitTarget()
    {
        //Debug.Log("hit ");
        shootHitsPerEpisode++;
        RegisterMetric(MetricKeys.Shoots_Hits.ToString(), MetricType.Int, shootHitsPerEpisode);

        AddReward(0.05f);
    }

    public override void Reset()
    {
        base.Reset();
        currentEnergy = startingEnergy;
        _robot.Reset();
    }


    #endregion
}