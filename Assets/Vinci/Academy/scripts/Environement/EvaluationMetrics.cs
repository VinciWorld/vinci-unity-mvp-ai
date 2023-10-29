using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum MetricType
{
    Int,
    Float,
    Percent,
    String,
    // Add more types as needed
}

[Serializable]
public class MetricValue
{
    public MetricType type;
    public object value;

    public MetricValue(MetricType type, object value)
    {
        this.type = type;
        this.value = value;
    }

    public string GetValueWithSymbol()
    {
        switch (type)
        {
            case MetricType.Int:
                return value.ToString();
            case MetricType.Float:
                return ((float)value).ToString("F2");  // 2 decimal places
            case MetricType.Percent:
                return ((float)value).ToString("P2");  // As percentage with 2 decimal places
            case MetricType.String:
                return value as string;
            default:
                return "Unknown Type";
        }
    }
}

[Serializable]
public class EvaluationMetrics
{
    public int EvaluationEpisodes = 10;

    public Dictionary<string, Dictionary<string, MetricValue>> commonEvaluationMetrics = new();
    public Dictionary<string, Dictionary<string, MetricValue>> envEvaluationMetrics = new();
    public List<Dictionary<string, Dictionary<string, MetricValue>>> metricsHistory = new();

    public void Initialize(string envID, Dictionary<string, MetricValue> envMetricsTemplate)
    {
        commonEvaluationMetrics[envID] = new Dictionary<string, MetricValue>
        {
            {"Goals Success", new MetricValue(MetricType.Int, 0)},
            {"Goals Failed", new MetricValue(MetricType.Int, 0)},
            {"Goal Success ratio", new MetricValue(MetricType.Percent, 0.0)}
        };

        envEvaluationMetrics[envID] = new Dictionary<string, MetricValue>(envMetricsTemplate);
    }

    public bool AddMetric(string envID, string metricName, MetricValue metricValue, bool isEnvironmentMetric = true)
    {
        var targetDict = isEnvironmentMetric ? envEvaluationMetrics : commonEvaluationMetrics;

        if (!targetDict.ContainsKey(envID))
        {
            targetDict[envID] = new Dictionary<string, MetricValue>();
        }

        if (!targetDict[envID].ContainsKey(metricName))
        {
            targetDict[envID][metricName] = metricValue;
            return true;
        }

        return false;
    }

    public bool UpdateMetric(string envID, string metricName, MetricValue metricValue, bool isEnvironmentMetric = true)
    {
        var targetDict = isEnvironmentMetric ? envEvaluationMetrics : commonEvaluationMetrics;

        if (targetDict.ContainsKey(envID) && targetDict[envID].ContainsKey(metricName))
        {
            targetDict[envID][metricName] = metricValue;
            return true;
        }

        return false;
    }

    public void StoreMetricsToHistory()
    {
        metricsHistory.Add(new Dictionary<string, Dictionary<string, MetricValue>>(envEvaluationMetrics));
    }

    public Dictionary<string, MetricValue> GetCommonMetrics(string envID)
    {
        if (commonEvaluationMetrics.TryGetValue(envID, out var metrics))
        {
            return metrics;
        }

        return null;
    }

    public Dictionary<string, MetricValue> GetEnvMetrics(string envID)
    {
        if (envEvaluationMetrics.TryGetValue(envID, out var metrics))
        {
            return metrics;
        }

        return null;
    }

    public Dictionary<string, MetricValue> GetZeroedCommonTemplate(string envID)
    {
        if (!commonEvaluationMetrics.ContainsKey(envID))
        {
            Debug.LogError($"Environment ID {envID} not found in envEvaluationMetrics!");
            return null;
        }

        var originalTemplate = commonEvaluationMetrics[envID];
        var zeroedTemplate = new Dictionary<string, MetricValue>();

        foreach (var item in originalTemplate)
        {
            object value = item.Value.type switch
            {
                MetricType.Int => 0,
                MetricType.Float => 0.0f,
                MetricType.Percent => 0.0f,
                MetricType.String => "",
                _ => null
            };
            zeroedTemplate[item.Key] = new MetricValue(item.Value.type, value);
        }
        return zeroedTemplate;
    }

    public Dictionary<string, MetricValue> GetZeroedEnvTemplate(string envID)
    {
        if (!envEvaluationMetrics.ContainsKey(envID))
        {
            Debug.LogError($"Environment ID {envID} not found in envEvaluationMetrics!");
            return null;
        }

        var originalTemplate = envEvaluationMetrics[envID];
        var zeroedTemplate = new Dictionary<string, MetricValue>();

        foreach (var item in originalTemplate)
        {
            object value = item.Value.type switch
            {
                MetricType.Int => 0,
                MetricType.Float => 0.0f,
                MetricType.Percent => 0.0f,
                MetricType.String => "",
                _ => null
            };
            zeroedTemplate[item.Key] = new MetricValue(item.Value.type, value);
        }
        return zeroedTemplate;
    }

    public void ProcessMetrics(string envID)
    {
        var metrics = GetEnvMetrics(envID);
        if (metrics == null) return;

        foreach (var kvp in metrics)
        {
            var metricName = kvp.Key;
            var metricValue = kvp.Value;

            Debug.Log($"Processing metric {metricName}: {metricValue.GetValueWithSymbol()}");
        }
    }
}