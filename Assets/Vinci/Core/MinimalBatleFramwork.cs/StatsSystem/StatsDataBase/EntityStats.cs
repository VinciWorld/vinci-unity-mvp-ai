using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace StatSystem
{

    public enum ProgressionType
    {
        levels,
        constant,
    }


    //[CreateAssetMenu(fileName = "EntityStats", menuName = "StatSystem/EntityStats", order = 0)]
    [System.Serializable]
    public class EntityStats
    {
        [SerializeField]
        [NonReorderable]
        public List<ProgressionStat> statsProgression;

        public ProgressionStat GetProgressionStat(StatType type)
        {
            return statsProgression.FirstOrDefault(s => s.statDefinition.type == type);

        }

        public int GetStatBaseValue(StatType type)
        {
            var stat = statsProgression.FirstOrDefault(s => s.statDefinition.type == type);

            return stat.levels[0];
        }
    }

    [System.Serializable]
    public class ProgressionStat
    {
        public ProgressionType progressionType;

        [SerializeField]
        public StatDefinition statDefinition;

        [NonReorderable]
        public List<int> levels;
    }
}
