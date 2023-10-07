using System.Collections.Generic;
using UnityEngine;



namespace StatSystem
{
    [CreateAssetMenu(fileName = "StatsDataBase", menuName = "StatSystem/StatsDataBase", order = 0)]
    public class StatsDataBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField]
        [NonReorderable]
        List<StatDefinition> _attributes = new List<StatDefinition>();
        public int NumberOfStats => _attributes.Count;

        [SerializeField]
        [NonReorderable]
        List<StatDefinition> _primaryStats = new List<StatDefinition>();

        [Header("Entities")]
        [SerializeField]
        [NonReorderable]
        List<EntitySO> entitiesDefinition;


        [ContextMenu("Refresh")]
        void Refresh()
        {
            if (entitiesDefinition.Count == 0) return;

            for (int i = 0; i < entitiesDefinition.Count; i++)
            {
                //RefreshStats(ref entitiesDefinition[i].stats.attributeProgression, entitiesDefinition[i].numberOfLevels);
            }
        }

        void RefreshStats(ref List<ProgressionStat> progressionStats, int numberOfLEvels)
        {
            if (progressionStats == null)
            {
                progressionStats = new List<ProgressionStat>(new ProgressionStat[_attributes.Count]);

            }
            else if (_attributes.Count != progressionStats.Count)
            {
                int diff = progressionStats.Count - _attributes.Count;
                if (diff < 0)
                {
                    List<ProgressionStat> newList = new List<ProgressionStat>(progressionStats);


                    newList.AddRange(new ProgressionStat[Mathf.Abs(diff)]);

                    progressionStats = newList;
                }
                else if (diff > 0)
                {
                    progressionStats.RemoveRange(progressionStats.Count - 1, diff);
                }
            }

            for (int j = 0; j < progressionStats.Count; j++)
            {
                if (progressionStats[j] == null)
                {
                    progressionStats[j] = new ProgressionStat();
                    progressionStats[j].statDefinition = _attributes[j];
                    //progressionStats[j].name = _attributes[j].name;
                    progressionStats[j].levels = new List<int>(new int[numberOfLEvels]);
                }
                else if (numberOfLEvels != progressionStats[j].levels.Count)
                {
                    int diff = progressionStats[j].levels.Count - numberOfLEvels;
                    Debug.Log($"dif = {diff}");
                    if (diff < 0)
                    {
                        List<int> newList = new List<int>(progressionStats[j].levels);
                        newList.AddRange(new int[Mathf.Abs(diff)]);
                        progressionStats[j].levels = newList;
                    }
                    else if (diff > 0)
                    {
                        progressionStats[j].levels.RemoveRange(numberOfLEvels - 1, diff);
                    }
                }
            }
        }
    

    }

  

}