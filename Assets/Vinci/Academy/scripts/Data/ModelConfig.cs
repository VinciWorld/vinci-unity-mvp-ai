using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;

public class EnvSpecificData
{
    public List<Dictionary<string, MetricValue>> commonEvaluationMetrics = new();
    public List<Dictionary<string, MetricValue>> envEvaluationMetrics = new();
    public List<Dictionary<int, Dictionary<string, MetricValue>>> agentEvaluationMetricsPerEpisode = new();
}

[Serializable]
public class ModelConfig
{
    // Properties
    public string runId;
    public BehaviorConfigSmall behavior;
    public TrainJobStatus trainJobStatus = TrainJobStatus.NONE;

    public string nnModelPath;
    public NNModel nnModel;
    public int trainCount => trainMetrics.Count;
    public int _totalStepsTrained;
    public int totalStepsTrained => _totalStepsTrained;

    // Flags
    public bool isModelLoaded = false;
    public bool isModelMinted = false;
    public bool isEvaluated = false;

    // Trian Metrics & Evaluations
    public List<ModelTrainMetric> trainMetrics = new();

    //Key envId
    public Dictionary<string, EnvSpecificData> envSpecificData = new();

    // Methods
    public ModelTrainMetric GetMostRecentMetric() => trainMetrics.LastOrDefault();
    public float GetLastMeanReward() => GetMostRecentMetric()?.GetLastMeanReward() ?? 0f;
    public float GetLastStdReward() => GetMostRecentMetric()?.GetLastStdReward() ?? 0f;
    public int GetStepsTrained() => GetMostRecentMetric()?.stepsTrained ?? 0;


    public void StoreSessionEvaluationMetrics(
        string envId,
        Dictionary<string, MetricValue> commonMetrics,
        Dictionary<string, MetricValue> envMetrics,
        Dictionary<int, Dictionary<string, MetricValue>> agentMetrics
    )
    {
        Dictionary<string, MetricValue> envMetricsAvg = EvaluationMetrics.ComputeAverageForEnvMetrics(envMetrics, agentMetrics);

        AddToCommonEvaluationMetrics(envId, commonMetrics);
        AddToEnvEvaluationMetrics(envId, envMetricsAvg);
        AddToAgentEvaluationMetricsPerEpisode(envId, agentMetrics);
    }

    public void AddToCommonEvaluationMetrics(string envKey, Dictionary<string, MetricValue> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvSpecificData();
        }
        envSpecificData[envKey].commonEvaluationMetrics.Add(metrics);
    }

    public void AddToEnvEvaluationMetrics(string envKey, Dictionary<string, MetricValue> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvSpecificData();
        }
        envSpecificData[envKey].envEvaluationMetrics.Add(metrics);
    }

    public void AddToAgentEvaluationMetricsPerEpisode(string envKey, Dictionary<int, Dictionary<string, MetricValue>> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvSpecificData();
        }

        envSpecificData[envKey].agentEvaluationMetricsPerEpisode.Add(metrics);
    }

    public Dictionary<string, MetricValue> GetCommonEvaluationMetrics(string envKey)
    {
        if (envSpecificData.ContainsKey(envKey))
        {
            return envSpecificData[envKey].commonEvaluationMetrics.LastOrDefault();
        }
        return null;
    }

    public Dictionary<string, MetricValue> GetEnvEvaluationMetrics(string envKey)
    {
        if (envSpecificData.ContainsKey(envKey))
        {
            return envSpecificData[envKey].envEvaluationMetrics.LastOrDefault();
        }
        return null;
    }

    public Dictionary<int, Dictionary<string, MetricValue>> GetAgentEvaluationMetricsPerEpisode(string envKey)
    {
        if (envSpecificData.ContainsKey(envKey))
        {
            return envSpecificData[envKey].agentEvaluationMetricsPerEpisode.LastOrDefault();
        }
        return null;
    }

    public Dictionary<string, MetricChange> GetCommonEvaluationMetricsChange(string envKey)
    {
        if (envSpecificData.ContainsKey(envKey))
        {
            List<Dictionary<string, MetricValue>> metricsList = envSpecificData[envKey].commonEvaluationMetrics;

            return EvaluationMetrics.CompareWithLastAndComputeChange(metricsList);
        }
        return null;
    }

    public Dictionary<string, MetricChange> GetEnvEvaluationMetricsChange(string envKey)
    {
        if (envSpecificData.ContainsKey(envKey))
        {
            List<Dictionary<string, MetricValue>> metricsList = envSpecificData[envKey].envEvaluationMetrics;

            return EvaluationMetrics.CompareWithLastAndComputeChange(metricsList);
        }
        return null;
    }

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
 
    public List<float> meanReward = new List<float>();
    public List<float> stdReward  = new List<float>();
    public int stepsTrained = 0;

    // Methods
    public float GetLastMeanReward() => meanReward.LastOrDefault();
    public float GetLastStdReward() => stdReward.LastOrDefault();
    public void AddMeanReward(float reward) => meanReward.Add(reward);
    public void AddStdReward(float reward) => stdReward.Add(reward);
}
