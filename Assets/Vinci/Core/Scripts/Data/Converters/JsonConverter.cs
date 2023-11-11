using System;
using System.Collections.Generic;
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