using System.Collections.Generic;
using UnityEngine;


namespace StatSystem
{
    [System.Serializable]
    public enum StatClassType
    {
        STAT,
        ATTRIBUTE,
    }

    [System.Serializable]
    public enum StatType
    {
        HEALTH,
        SPEED,
        ATTACK,
        ENERGY,
        DAMAGE,
        RANGE,
        FIRERATEPERSECOND,
        ARMOR,
        COST,


        PROCESSING_POWER,
        MECHANICAL_DEXTERITY,
        ENERGY_EFFICIENCY,
        SENSOR_ACCURACY,
        DATA_STORAGE
    }


    [System.Serializable]
    public class AgregatedStats
    {   
        public StatDefinition stat;
        public float defaultValue = 100f;
        public float weightFactor = 1f;
    }

    [CreateAssetMenu(fileName = "StatDefinition", menuName = "StatSystem/StatDefinition", order = 0)]
    public class StatDefinition : ScriptableObject
    {
        public string statName => base.name;

        [SerializeField]
        private StatClassType _statClassType;
        public StatClassType statClassType => _statClassType;

        [SerializeField]
        private StatType _type;
        public StatType type => _type;

        [SerializeField]
        private string _alias;
        public string alias => _alias;

        [SerializeField]
        protected string _description;
        public string description => _description;

        [SerializeField]
        private int _UUID;
        public int UUID => _UUID;

        [SerializeField]
        List<AgregatedStats> agregatedStats;

    }
}
