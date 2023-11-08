using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;

[Serializable]
public class EnvMetricsData
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

    // Model Train Metrics
    public List<ModelTrainMetrics> trainMetricsHistory = new();

    //Key envId
    public Dictionary<string, EnvMetricsData> envSpecificData = new();

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
        Dictionary<string, MetricValue> envMetricsAvg = EvaluationMetrics.ComputeAgentMetricsSumForEnvMetrics(envMetrics, agentMetrics);

        AddToCommonEvaluationMetrics(envId, commonMetrics);
        AddToEnvEvaluationMetrics(envId, envMetricsAvg);
        AddToAgentEvaluationMetricsPerEpisode(envId, agentMetrics);
    }

    public void AddToCommonEvaluationMetrics(string envKey, Dictionary<string, MetricValue> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvMetricsData();
        }
        envSpecificData[envKey].commonEvaluationMetrics.Add(metrics);
    }

    public void AddToEnvEvaluationMetrics(string envKey, Dictionary<string, MetricValue> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvMetricsData();
        }
        envSpecificData[envKey].envEvaluationMetrics.Add(metrics);
    }

    public void AddToAgentEvaluationMetricsPerEpisode(string envKey, Dictionary<int, Dictionary<string, MetricValue>> metrics)
    {
        if (!envSpecificData.ContainsKey(envKey))
        {
            envSpecificData[envKey] = new EnvMetricsData();
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
        ResetLastTrainMetricsEntry();

        var trainMetric = new ModelTrainMetrics();
        trainMetricsHistory.Add(trainMetric);
    }

    public void CreateNewTrainMetricsEntry(List<MetricsData> metrics)
    {
        ResetLastTrainMetricsEntry();

        trainMetricsHistory.Add(new ModelTrainMetrics(metrics));
    }

    private void ResetLastTrainMetricsEntry()
    {
        var lastMetric = GetMostRecentMetricsHistory();
        if(lastMetric != null)
        {   if (!lastMetric.completed)
                trainMetricsHistory.Remove(lastMetric);
        }
    }

    public void AddStepsTrained(int stepsTrained)
    {
        _totalStepsTrained += stepsTrained;
        var lastMetric = GetMostRecentMetricsHistory();
        if (lastMetric != null)
        {
            lastMetric.stepsTrained = stepsTrained;
            lastMetric.completed = true;
        }
    }

    public void AddTrainMetricsEntry(int setpCount, float meanReward, float stdReward, float timeElapsed)
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
    // List<MetricsData> _metrics = new List<MetricsData>();

    public List<int> stepCount = new List<int>();
    public List<float> meanRewardList = new List<float>();
    public List<float> stdRewardList  = new List<float>();
    public List<float> timeElapsedList = new List<float>();
    public int stepsTrained = 0;
    public bool completed = false;

    public ModelTrainMetrics(){}
    public ModelTrainMetrics(List<MetricsData> metrics)
    {
        foreach (var item in metrics)
        {
            stepCount.Add(item.step);
            meanRewardList.Add(item.mean_reward);
            stdRewardList.Add(item.std_reward);
            timeElapsedList.Add(item.time_elapsed);

        }
        stepsTrained = stepCount.LastOrDefault();
        completed = true;
    }

    public float GetLastStepCount() => stepCount.LastOrDefault();
    public float GetLastMeanReward() => meanRewardList.LastOrDefault();
    public float GetLastStdReward() => stdRewardList.LastOrDefault();
    public float GetLastTimeElapsed() => timeElapsedList.LastOrDefault();
    public void AddStepCount(int setpCount) => stepCount.Add(setpCount);
    public void AddMeanReward(float meanReward) => meanRewardList.Add(meanReward);
    public void AddStdReward(float stdReward) => stdRewardList.Add(stdReward);
    public void AddTimeElapsed(float timeElapsed) => timeElapsedList.Add(timeElapsed);

    public void Reset()
    {
        stepCount.Clear();
        meanRewardList.Clear();
        stdRewardList.Clear();
        timeElapsedList.Clear();
        stepsTrained = 0;
    }
}
