using System.Collections.Generic;

using StatSystem;
using UnityEngine;


public class EntitySO : MonoBehaviour
{
    public string name;
    [SerializeField]
    public string _alias;
    public string alias => _alias;

    [SerializeField]
    public string _description;
    public string description => _description;

    [SerializeField]

    public int _numberOfLevels = 0;
   
    public int numberOfLevels => _numberOfLevels;

    [SerializeField]
    public EntityStats _entityStats;

    void Rebuild()
    {
        /*
        if(_entityStats == null) return;
        
        if(_entityStats.statsGroup != null)
        {
            Dictionary<StatClassType, List<StatDefinition>> statsTypes = _entityStats.statsGroup.statsTypes;

            foreach (StatClassType key in statsTypes.Keys)
            {
                List<StatDefinition> statsDefinitions = statsTypes[key];
                List<ProgressionStat> progressionStats = new List<ProgressionStat>();

                foreach(StatDefinition statDefinition in statsTypes[key])
                {
                    ProgressionStat progressionStat = new ProgressionStat();
                    progressionStat.name = statDefinition.name;
                    progressionStat.statDefinition = statDefinition;
                    progressionStat.levels = new List<int>(new int[numberOfLevels]);

                    progressionStats.Add(progressionStat);
                }

                if(key == StatClassType.ATTRIBUTE || key == StatClassType.PRIMARY_STAT)
                {
                    _entityStats.statsProgression = progressionStats;
                }
                else
                {
                    _entityStats.dependentStatsProgression = progressionStats;
                }
            }
        }
        */
    }
}

