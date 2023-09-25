using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Utils;


public class ResourcesManager : Singleton<ResourcesManager>
{   
/*
    public List<AgentConfig> GetAgentsConfigByType(AgentType tpye)
    {
        List<AgentConfig> agentsConfig = new List<AgentConfig>();

        AgentConfig[] agents = Resources.LoadAll<AgentConfig>("Agents/Configs");

        foreach (var agentConfig in agents)
        {
            if(agentConfig.agentDefaultData.type == tpye)
            {
                agentsConfig.Add(agentConfig);
            }
        }

        return agentsConfig;
    }

    public Sprite GetSprite(string spriteReference)
    {
        return Resources.Load<Sprite>("Ui/Sprites/" + spriteReference) as Sprite;
    }

    */
}