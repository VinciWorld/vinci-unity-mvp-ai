using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;


[Serializable]
public class ModelConfig
{
    // Properties
    public string runId;
    public BehaviorConfigSmall behavior;
    public TrainJobStatus trainJobStatus;
    public string nnModelPath;
    public NNModel nnModel;
    public int trainCount => trainMetrics.Count;
    public int _totalStepsTrained;
    public int totalStepsTrained => _totalStepsTrained;

    // Training Flags
    // public bool isModelSubmitted = false;
    // public bool isModelTraining  = false;
    // public bool isModelSucceeded  = false;

    // public bool modelFinishedTraining = false;
    public bool isModelLoaded = false;
    public bool isModelMinted = false;
    public bool isEvaluated = false;

    // Metrics & Evaluations
    public List<ModelTrainMetric> trainMetrics = new();
    public Dictionary<string, Dictionary<string, string>> modelEnvsEvaluationsResults = new();

    // Methods
    public ModelTrainMetric GetMostRecentMetric() => trainMetrics.LastOrDefault();

    public float GetLastMeanReward() => GetMostRecentMetric()?.GetLastMeanReward() ?? 0f;
    public float GetLastStdReward() => GetMostRecentMetric()?.GetLastStdReward() ?? 0f;
    public int GetStepsTrained() => GetMostRecentMetric()?.stepsTrained ?? 0;


    public void CreateNewTrainMetricsEntry()
    {
        var trainMetric = new ModelTrainMetric();
        trainMetrics.Add(trainMetric);
    }

    public void ResetLastTrainMetricsEntry()
    {
        var lastMetric = GetMostRecentMetric();
        lastMetric.meanReward.Clear();
        lastMetric.stdReward.Clear();
        lastMetric.stepsTrained = 0;
    }

    public void AddStepsTrained(int stepsTrained)
    {
        _totalStepsTrained += stepsTrained;
        var lastMetric = GetMostRecentMetric();
        if (lastMetric != null)
        {
            lastMetric.stepsTrained = stepsTrained;
        }
    }

    public void AddTrainMetrics(float meanReward, float stdReward)
    {
        var lastMetric = GetMostRecentMetric();
        if (lastMetric != null)
        {
            lastMetric.AddMeanReward(meanReward);
            lastMetric.AddStdReward(stdReward);
        }
    }

}

[Serializable]
public class ModelTrainMetric
{
    // Properties
 
    public List<float> meanReward { get; set; } = new List<float>();
    public List<float> stdReward { get; set; } = new List<float>();
    public int stepsTrained { get; set; } = 0;

    // Methods
    public float GetLastMeanReward() => meanReward.LastOrDefault();
    public float GetLastStdReward() => stdReward.LastOrDefault();
    public void AddMeanReward(float reward) => meanReward.Add(reward);
    public void AddStdReward(float reward) => stdReward.Add(reward);
}
