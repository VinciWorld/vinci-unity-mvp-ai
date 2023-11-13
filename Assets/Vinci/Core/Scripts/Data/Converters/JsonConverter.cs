using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vinci.Core.Managers;


public class AgentBlueprintConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(AgentBlueprint);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JArray jsonArray = JArray.Load(reader);

        List<AgentBlueprint> agents = new List<AgentBlueprint>();

        foreach (JObject jsonObject in jsonArray)
        {
            string agentId = jsonObject["agent_id"].ToObject<string>();
            ModelConfig modelConfig = jsonObject["model_config"].ToObject<ModelConfig>();

            AgentBlueprint agent = GameManager.instance.gameData.CreateInstanceById(agentId);
            agent.modelConfig = modelConfig;

            agents.Add(agent);
        }

        return agents;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        List<AgentBlueprint> agents = (List<AgentBlueprint>)value;

        JArray agentsArray = new JArray();

        foreach (var agent in agents)
        {
            JObject agentObject = new JObject();
            agentObject.Add("agent_id", JToken.FromObject(agent.id));
            agentObject.Add("model_config", JToken.FromObject(agent.modelConfig));

            agentsArray.Add(agentObject);
        }

        agentsArray.WriteTo(writer);
    }
}

public class EnvMetricsDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(EnvMetricsData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        EnvMetricsData envMetricsData = new EnvMetricsData();

        // Deserialize each property of EnvMetricsData
        envMetricsData.commonEvaluationMetrics = jsonObject["commonEvaluationMetrics"].ToObject<List<Dictionary<string, MetricValue>>>(serializer);
        envMetricsData.envEvaluationMetrics = jsonObject["envEvaluationMetrics"].ToObject<List<Dictionary<string, MetricValue>>>(serializer);
        envMetricsData.agentEvaluationMetricsPerEpisode = jsonObject["agentEvaluationMetricsPerEpisode"].ToObject<List<Dictionary<int, Dictionary<string, MetricValue>>>>(serializer);

        return envMetricsData;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var envMetricsData = (EnvMetricsData)value;
        JObject jsonObject = new JObject
        {
            ["commonEvaluationMetrics"] = JToken.FromObject(envMetricsData.commonEvaluationMetrics, serializer),
            ["envEvaluationMetrics"] = JToken.FromObject(envMetricsData.envEvaluationMetrics, serializer),
            ["agentEvaluationMetricsPerEpisode"] = JToken.FromObject(envMetricsData.agentEvaluationMetricsPerEpisode, serializer)
        };

        jsonObject.WriteTo(writer);
    }
}

public class MetricValueConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(MetricValue);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var metricValue = (MetricValue)value;
        var token = JToken.FromObject(metricValue.value);
        var obj = new JObject
        {
            ["type"] = JToken.FromObject(metricValue.type),
            ["value"] = token,
            ["IsHigherBetter"] = metricValue.IsHigherBetter
        };
        obj.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        MetricType type = obj["type"].ToObject<MetricType>();
        object val = obj["value"].ToObject(GetType(type));
        bool isHigherBetter = obj["IsHigherBetter"].ToObject<bool>();

        return new MetricValue(type, val, isHigherBetter);
    }

    private Type GetType(MetricType metricType)
    {
        switch (metricType)
        {
            case MetricType.Int:
                return typeof(int);
            case MetricType.Float:
                return typeof(float);
            case MetricType.Percent:
                return typeof(float);
            case MetricType.String:
                return typeof(string);
            default:
                throw new ArgumentException("Unknown MetricType");
        }
    }
}