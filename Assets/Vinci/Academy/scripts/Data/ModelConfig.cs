using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

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
    public ModelAsset nnModel;
    public int trainCount => trainMetricsHistory.Count;
    public int _totalStepsTrained;
    public int totalStepsTrained => _totalStepsTrained;

    // Flags
    public bool isModelLoaded = false;
    public bool isModelMinted = false;
    public bool isEvaluated = false;

    // Trian Metrics & Evaluations
    public List<ModelTrainMetrics> trainMetricsHistory = new();

    //Key envId
    public Dictionary<string, EnvSpecificData> envSpecificData = new();

    // Methods
    public ModelTrainMetrics GetMostRecentMetricsHistory() => trainMetricsHistory.LastOrDefault();
    public float GetLastMeanReward() => GetMostRecentMetricsHistory()?.GetLastMeanReward() ?? 0f;
    public float GetLastStdReward() => GetMostRecentMetricsHistory()?.GetLastStdReward() ?? 0f;
    public int GetStepsTrained() => GetMostRecentMetricsHistory()?.stepsTrained ?? 0;


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
        var trainMetric = new ModelTrainMetrics();
        trainMetricsHistory.Add(trainMetric);
    }

    public void AddTrainMetrics(List<MetricsData> metrics)
    {
        trainMetricsHistory.Add(new ModelTrainMetrics(metrics));
    }

    public void ResetLastTrainMetricsEntry()
    {
        var lastMetric = GetMostRecentMetricsHistory();
        lastMetric.Reset();

    }

    public void AddStepsTrained(int stepsTrained)
    {
        _totalStepsTrained += stepsTrained;
        var lastMetric = GetMostRecentMetricsHistory();
        if (lastMetric != null)
        {
            lastMetric.stepsTrained = stepsTrained;
        }
    }

    public void AddTrainMetrics(int setpCount, float meanReward, float stdReward, float timeElapsed)
    {
        var lastMetric = GetMostRecentMetricsHistory();
        if (lastMetric != null)
        {
            lastMetric.AddStepCount(setpCount);
            lastMetric.AddMeanReward(meanReward);
            lastMetric.AddStdReward(stdReward);
            lastMetric.AddTimeElapsed(timeElapsed);
        }
    }
}

[Serializable]
public class ModelTrainMetrics
{
    // Properties
    List<MetricsData> _metrics;

    public List<int> stepCount = new List<int>();
    public List<float> meanReward = new List<float>();
    public List<float> stdReward  = new List<float>();
    public List<float> timeElapsedList = new List<float>();
    public int stepsTrained = 0;

    public ModelTrainMetrics(){}
    public ModelTrainMetrics(List<MetricsData> metrics)
    {
        _metrics = metrics;
    }

    // Methods
    public float GetLastStepCount() => _metrics.LastOrDefault().step;
    public float GetLastMeanReward() => _metrics.LastOrDefault().mean_reward;
    public float GetLastStdReward() => _metrics.LastOrDefault().std_reward;
    public float GetLastTimeElapsed() => _metrics.LastOrDefault().time_elapsed;
    public void AddStepCount(int setpCount) => stepCount.Add(setpCount);
    public void AddMeanReward(float reward) => meanReward.Add(reward);
    public void AddStdReward(float reward) => stdReward.Add(reward);
    public void AddTimeElapsed(float timeElapsed) => timeElapsedList.Add(timeElapsed);

    public void Reset()
    {
        stepCount.Clear();
        meanReward.Clear();
        stdReward.Clear();
        timeElapsedList.Clear();
        stepsTrained = 0;
    }
}
