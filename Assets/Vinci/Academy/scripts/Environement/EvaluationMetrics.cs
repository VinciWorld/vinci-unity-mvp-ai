using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using WebSocketSharp;
using Newtonsoft.Json;

[Serializable]
public enum MetricType
{
    Int,
    Float,
    Percent,
    String,
}

[Serializable]
[JsonConverter(typeof(MetricValueConverter))]
public class MetricValue
{
    public MetricType type;
    public object value;
    public bool IsHigherBetter;

    public MetricValue(MetricType type, object value, bool isHigherBetter = true)
    {
        this.type = type;
        this.value = value;
        this.IsHigherBetter = isHigherBetter;
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
                return ((float)value).ToString("P1");  // As percentage with 2 decimal places
            case MetricType.String:
                return value as string;
            default:
                return "Unknown Type";
        }
    }
}

[Serializable]
public class MetricChange
{
    public MetricValue currentValue;
    public object change;
    public ChangeStatus status;

    public MetricChange(MetricValue currentValue, object change, ChangeStatus status)
    {
        this.currentValue = currentValue;
        this.change = change;
        this.status = status;
    }

    public string GetValueWithSymbol()
    {
        switch (currentValue.type)
        {
            case MetricType.Int:
                return currentValue.value.ToString();
            case MetricType.Float:
                return ((float)currentValue.value).ToString("F2");
            case MetricType.Percent:
                return ((float)currentValue.value).ToString("P1");
            case MetricType.String:
                return currentValue.value as string;
            default:
                return "Unknown Type";
        }
    }

    public string GetValueChangeWithSymbol()
    {
        if (change is string && ((string)change).IsNullOrEmpty())
            return change as string;

        switch (currentValue.type)
        {
            case MetricType.Int:
                return CheckChangeStatusSymbol(change.ToString());
            case MetricType.Float:
                return CheckChangeStatusSymbol(((float)change).ToString("F2"));
            case MetricType.Percent:
                return CheckChangeStatusSymbol(((float)change).ToString("P1"));
            case MetricType.String:
                return CheckChangeStatusSymbol(change as string);
            default:
                return "Unknown Type";
        }
    }

    private string CheckChangeStatusSymbol(string change)
    {
        switch (status)
        {
            case ChangeStatus.Better:
                if(currentValue.IsHigherBetter)
                {
                    return "+" + change;
                }
                return change;

            case ChangeStatus.Same:
                return change;
            case ChangeStatus.Worse:
                if (currentValue.IsHigherBetter)
                {
                    return change;
                }
                return "+" + change;
        }

        return change;
    }
}

[Serializable]
public enum ChangeStatus
{
    Better,
    Worse,
    Same
}

[Serializable]
public class EvaluationMetrics
{
    private static readonly Dictionary<string, MetricValue> commonMetricsTemplate = new Dictionary<string, MetricValue>
    {
        {"Goals Success", new MetricValue(MetricType.Int, 0)},
        {"Goals Failed", new MetricValue(MetricType.Int, 0)},
        {"Goals Success ratio", new MetricValue(MetricType.Percent, 0.0f)}
    };

    public Dictionary<string, MetricValue> commonEvaluationMetrics = new();
    public Dictionary<string, MetricValue> envEvaluationMetrics = new();
    public Dictionary<string, MetricValue> agentEvaluationMetrics = new();

    public Dictionary<int, Dictionary<string, MetricValue>> agentEvaluationMetricsPerEpisode = new();

    public event Action<Dictionary<string, MetricValue>> commonMetricsUpdated;
    public event Action<Dictionary<string, MetricValue>> envMetricsUpdated;
    public event Action<Dictionary<string, MetricValue>> agentMetricsUpdated;


    public void Initialize(Dictionary<string, MetricValue> envMetricsTemplate,
        Dictionary<string, MetricValue> agentMetricsTemplate
    )
    {
        commonEvaluationMetrics = new Dictionary<string, MetricValue>(commonMetricsTemplate);
        envEvaluationMetrics = new Dictionary<string, MetricValue>(envMetricsTemplate);
        agentEvaluationMetrics = new Dictionary<string, MetricValue>(agentMetricsTemplate);
    }

    public bool UpdateAgentMetricForEpisode(int episode, string metricKey, MetricValue metricValue)
    {
        if (!agentEvaluationMetricsPerEpisode.TryGetValue(episode, out var metrics))
        {
            // If the episode doesn't exist, initialize it
            metrics = new Dictionary<string, MetricValue>();
            foreach (var metric in agentEvaluationMetrics)
            {
                metrics[metric.Key] = new MetricValue(metric.Value.type, metric.Value.value, metric.Value.IsHigherBetter);
            
            }
            agentEvaluationMetricsPerEpisode[episode] = metrics;
        }

        if (metrics.ContainsKey(metricKey))
        {
            metrics[metricKey] = metricValue;
            agentMetricsUpdated?.Invoke(GetAgentMetricsForEpisode(episode));
            return true;
        }
        else
        {
            Debug.LogWarning($"Metric key {metricKey} not found for episode {episode}. Skipping...");
            return false;
        }
    }

    public bool UpdateEnvMetric(string metricName, MetricValue metricValue)
    {
        if (envEvaluationMetrics.ContainsKey(metricName))
        {
            envEvaluationMetrics[metricName] = metricValue;
            return true;
        }
        Debug.LogWarning($"Metric {metricName} not found. Skipping...");
        return false;
    }

    public bool UpdateEnvMetrics(Dictionary<string, object> metricsToUpdate)
    {
        foreach (var metric in metricsToUpdate)
        {
            if (envEvaluationMetrics.ContainsKey(metric.Key))
            {
                envEvaluationMetrics[metric.Key].value = metric.Value;
            }
            else
            {
                Debug.LogWarning($"Metric {metric.Key} not found. Skipping...");
            }
        }

        envMetricsUpdated?.Invoke(GetEnvMetrics());
        return true;
    }

    private void SetCommonMetricValue(string metricName, object value)
    {
        if (commonMetricsTemplate.ContainsKey(metricName))
        {
            commonEvaluationMetrics[metricName].value = value;
        }
        else
        {
            Debug.LogWarning($"Metric {metricName} not found. Skipping...");
        }
    }

    public void UpdateCommonMetrics(int goallSuccess, int goalFailed, float successRatio)
    {
        SetCommonMetricValue("Goals Success", goallSuccess);
        SetCommonMetricValue("Goals Failed", goalFailed);
        SetCommonMetricValue("Goals Success ratio", successRatio);

        commonMetricsUpdated?.Invoke(GetCommonMetrics());
    }

    public void SetGoalsSuccess(int value)
    {
        SetCommonMetricValue("Goals Success", value);
    }
    public void SetGoalsFailed(int value)
    {
        SetCommonMetricValue("Goals Failed", value);
    }
    public void SetGoalsSuccessRatio(float value)
    {
        SetCommonMetricValue("Goals Success", value);
    }

    public void StoreMetricsSnapshot(string envId, string agentInstance)
    {

    }

    public Dictionary<string, MetricValue> GetCommonMetrics()
    {

        return commonEvaluationMetrics;
    }

    public Dictionary<string, MetricValue> GetEnvMetrics()
    {

        return envEvaluationMetrics;;

    }

    public Dictionary<int, Dictionary<string, MetricValue>> GetAgentetrics()
    {

        return agentEvaluationMetricsPerEpisode; ;

    }

    public Dictionary<string, MetricValue> GetAgentMetricsForEpisode(int episode)
    {
        if (agentEvaluationMetricsPerEpisode.TryGetValue(episode, out var episodeMetrics))
        {   

            return episodeMetrics;
        }

        Debug.LogWarning($"Metrics not found for Episode {episode}");
        return null;
    }

    public Dictionary<string, MetricValue> GetZeroedCommonTemplate()
    {

        var originalTemplate = commonEvaluationMetrics;
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

    public Dictionary<string, MetricValue> GetZeroedEnvTemplate()
    {
        var originalTemplate = envEvaluationMetrics;
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



    public static MetricChange ComputeMetricChange(MetricValue currentValue, MetricValue oldValue)
    {
        object change;
        ChangeStatus status = ChangeStatus.Same;

        switch (currentValue.type)
        {
            case MetricType.Int:
                change = (int)currentValue.value - (int)oldValue.value;
                status = DetermineStatus((int)change, currentValue.IsHigherBetter);
                break;
            case MetricType.Float:
                change = (float)currentValue.value - (float)oldValue.value;
                status = DetermineStatus((float)change, currentValue.IsHigherBetter);
                break;
            case MetricType.Percent:
                change = ((float)currentValue.value / 100.0f - (float)oldValue.value / 100.0f) * 100.0f;
                status = DetermineStatus((float)change, currentValue.IsHigherBetter);
                break;
            case MetricType.String:
                change = (currentValue.value.ToString() == oldValue.value.ToString()) ? "Unchanged" : "Changed";
                break;
            default:
                change = null;
                break;
        }

        return new MetricChange(currentValue, change, status);
    }

    private static ChangeStatus DetermineStatus(float change, bool isHigherBetter)
    {
        if (change == 0) return ChangeStatus.Same;
        if (isHigherBetter) return change > 0 ? ChangeStatus.Better : ChangeStatus.Worse;
        return change < 0 ? ChangeStatus.Better : ChangeStatus.Worse;
    }

    private static ChangeStatus DetermineStatus(int change, bool isHigherBetter)
    {
        if (change == 0) return ChangeStatus.Same;
        if (isHigherBetter) return change > 0 ? ChangeStatus.Better : ChangeStatus.Worse;
        return change < 0 ? ChangeStatus.Better : ChangeStatus.Worse;
    }

    public static Dictionary<string, MetricChange> CompareWithLastAndComputeChange(List<Dictionary<string, MetricValue>> targetHistory)
    {
        Dictionary<string, MetricValue> lastMetrics;
        Dictionary<string, MetricValue> secondToLastMetrics;

        if (targetHistory.Count == 1)
        {
            lastMetrics = targetHistory[0];
            secondToLastMetrics = null;
        }
        else
        {
            lastMetrics = targetHistory[targetHistory.Count - 1];
            secondToLastMetrics = targetHistory[targetHistory.Count - 2];
        }

        var result = new Dictionary<string, MetricChange>();

        foreach (var metric in lastMetrics)
        {
            if (secondToLastMetrics != null && secondToLastMetrics.ContainsKey(metric.Key))
            {
                var oldValue = secondToLastMetrics[metric.Key];
                var currentValue = metric.Value;
                var change = ComputeMetricChange(currentValue, oldValue);

                result[metric.Key] = change;
            }
            else
            {
                var change = new MetricChange
                (
                    metric.Value,
                    "",
                    ChangeStatus.Same
                );

                result[metric.Key] = change;
            }
        }
        return result;
    }

    public static Dictionary<string, MetricValue> ComputeAgentMetricsSumForEnvMetrics(
    Dictionary<string, MetricValue> envMetrics,
    Dictionary<int, Dictionary<string, MetricValue>> agentMetrics
)
    {
        foreach (var key in envMetrics.Keys.ToList())
        {
            double totalValue = 0.0;
            int count = 0;

            foreach (var episodeMetrics in agentMetrics.Values)
            {
                if (episodeMetrics.ContainsKey(key))
                {
                    switch (episodeMetrics[key].type)
                    {
                        case MetricType.Float:
                            totalValue += (float)episodeMetrics[key].value;
                            count++;
                            break;
                        case MetricType.Int:
                            totalValue += (int)episodeMetrics[key].value;
                            count++;
                            break;
                    }
                }
            }

            if (envMetrics[key].type == MetricType.Float)
            {
                envMetrics[key] = new MetricValue(MetricType.Float, (float)totalValue, envMetrics[key].IsHigherBetter);
            }
            else if (envMetrics[key].type == MetricType.Percent)
            {
                envMetrics[key] = new MetricValue(MetricType.Float, (float)totalValue, envMetrics[key].IsHigherBetter);
            }
            else if (envMetrics[key].type == MetricType.Int)
            {
                Debug.Log("sum: " + totalValue);
                envMetrics[key] = new MetricValue(MetricType.Int, (int)Math.Round(totalValue), envMetrics[key].IsHigherBetter);
            }
        }

        return envMetrics;
    }


    public static Dictionary<string, MetricValue> ComputeAgentMetricsAverageForEnvMetrics(
        Dictionary<string, MetricValue> envMetrics,
        Dictionary<int, Dictionary<string, MetricValue>> agentMetrics
    )
    {
        foreach (var key in envMetrics.Keys.ToList())
        {
            double totalValue = 0.0;
            int count = 0;

            foreach (var episodeMetrics in agentMetrics.Values)
            {
                if (episodeMetrics.ContainsKey(key))
                {
                    switch (episodeMetrics[key].type)
                    {
                        case MetricType.Float:
                            totalValue += (float)episodeMetrics[key].value;
                            count++;
                            break;
                        case MetricType.Int:
                            totalValue += (int)episodeMetrics[key].value;
                            count++;
                            break;
                    }
                }
            }

            if (count > 0)
            {
                double averageValue = totalValue / count;

                if (envMetrics[key].type == MetricType.Float)
                {
                    envMetrics[key] = new MetricValue(MetricType.Float, (float)averageValue, envMetrics[key].IsHigherBetter);
                }
                else if (envMetrics[key].type == MetricType.Percent)
                {
                    envMetrics[key] = new MetricValue(MetricType.Float, (float)averageValue, envMetrics[key].IsHigherBetter);
                }
                else if (envMetrics[key].type == MetricType.Int)
                {
                    envMetrics[key] = new MetricValue(MetricType.Int, (int)Math.Round(averageValue), envMetrics[key].IsHigherBetter);
                }
            }
        }

        return envMetrics;
    }

    public void Reset()
    {
        agentEvaluationMetricsPerEpisode.Clear();
    }

    public static Dictionary<string, MetricChange> CompareAgentMetricsWithPreviousEpisode(Dictionary<int, Dictionary<string, MetricValue>> agentMetrics)
    {
        if (agentMetrics.Count == 0) return new Dictionary<string, MetricChange>();

        int lastEpisodeNumber = agentMetrics.Keys.Max();
        int secondToLastEpisodeNumber = agentMetrics.Keys.Where(k => k < lastEpisodeNumber).DefaultIfEmpty().Max();

        Dictionary<string, MetricValue> lastMetrics = agentMetrics[lastEpisodeNumber];
        Dictionary<string, MetricValue> secondToLastMetrics;

        if (agentMetrics.ContainsKey(secondToLastEpisodeNumber))
        {
            secondToLastMetrics = agentMetrics[secondToLastEpisodeNumber];
        }
        else
        {
            secondToLastMetrics = null;
        }

        var result = new Dictionary<string, MetricChange>();

        foreach (var metric in lastMetrics)
        {
            if (secondToLastMetrics != null && secondToLastMetrics.ContainsKey(metric.Key))
            {
                var oldValue = secondToLastMetrics[metric.Key];
                var currentValue = metric.Value;
                var change = ComputeMetricChange(currentValue, oldValue);

                result[metric.Key] = change;
            }
            else
            {
                var change = new MetricChange
                (
                    metric.Value,
                    null,
                    ChangeStatus.Same
                );

                result[metric.Key] = change;
            }
        }
        return result;
    }

    public static string ConvertEnumToString(MetricKeys key)
    {
        return key.ToString().Replace('_', ' ').ToLower();
    }

    public void ProcessMetrics()
    {
        var metrics = GetEnvMetrics();
        if (metrics == null) return;

        foreach (var kvp in metrics)
        {
            var metricName = kvp.Key;
            var metricValue = kvp.Value;

            Debug.Log($"Processing metric {metricName}: {metricValue.GetValueWithSymbol()}");
        }
    }
}


/*
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
*/