using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatSystem
{
    public class StatController : MonoBehaviour
    {
        [Range(0, 100)]
        [SerializeField]
        private int _level = 0;

        [SerializeField]
        private EntityStats _entityStats;

        protected Dictionary<StatType, Attribute> _attributes = new Dictionary<StatType, Attribute>();
        protected Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

        private bool isInitialized = false;

        private void Awake() 
        {
            
        }

        public void Initialize()
        {

            foreach(ProgressionStat progression in _entityStats.statsProgression)
            {
                if(progression.statDefinition.statClassType == StatClassType.ATTRIBUTE)
                {
                    Attribute newAttribue = new Attribute(progression, this);
                    newAttribue.Initialize(_level);

                    _attributes.Add(progression.statDefinition.type, newAttribue);

                }
                else
                {
                    Stat newStat = new Stat(progression, this);
                    newStat.Initialize(_level);
                    _stats.Add(progression.statDefinition.type, newStat);
                }
            }
            isInitialized = true;
        }

        public Attribute GetAttribute(StatType type)
        {
            if(!isInitialized)
            {
                Initialize();
            }

            Attribute attribute;
            _attributes.TryGetValue(type, out attribute);
            return attribute;
        }

        public void ResetAttributes()
        {
            foreach(var attirbute in _attributes)
            {
                attirbute.Value.Reset();
            }
        }

        public Stat GetStat(StatType type)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            Stat stat;
            _stats.TryGetValue(type, out stat);

            return stat;
        } 
    }
}

