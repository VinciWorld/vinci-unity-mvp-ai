using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatSystem
{

    public class Stat
    {
        protected ProgressionStat _statProgression;
        protected StatController _controller;

        protected float _baseValue;
        public float baseValue => _baseValue;

        protected float _modifiersValue;
        public float modifiersValue => _modifiersValue;

        public float totalValue => baseValue + _modifiersValue;

        //Event with modifiers value and current total value
        public event Action<float, float> baseValueChanged;


        [SerializeField]
        List<IModifier> _modifiers = new List<IModifier>();

        public Stat(ProgressionStat statProgression, StatController controller)
        {
            _statProgression = statProgression;
            _controller = controller;
            
        }

        public virtual void Initialize(int level)
        {
            _baseValue = GetInitialBaseValue(level);
            CalculateValue();
        }
        
        public void PermanentChangeOnBaseValue(IModifier modifier)
        {
           // modifier.Apply(ref _baseValue);
        }

        public void AddModifier(IModifier modifier)
        {
            _modifiers.Add(modifier);
            CalculateValue();
        }

        public void RemoveModifier(IModifier modifier)
        {
            _modifiers.Remove(modifier);
            CalculateValue();
        }

        internal void CalculateValue()
        {
            if(_modifiers.Count == 0) return;

            float lastModifiersValue = _modifiersValue;
            _modifiersValue = 0;

            _modifiers.Sort((x, y) => x.CompareTo(y));

            for (int i = 0; i < _modifiers.Count; i++)
            {
                _modifiers[i].Apply(ref _modifiersValue);
            }

            //TODO: When the maxCAp change, we need to check if the currentValue on attribute is above
            if(lastModifiersValue != _modifiersValue)
            {
                baseValueChanged?.Invoke(_modifiersValue, baseValue + _modifiersValue);
            }
        }

        public virtual float GetInitialBaseValue(int level)
        {
            if(level > _statProgression.levels.Count) return 0;

            return _statProgression.levels[level];
        }

        public void SetLevel(int level)
        {
            _baseValue = _statProgression.levels[level];
            CalculateValue();
        }

        public virtual void Reset()
        {
            _modifiers.Clear();
            CalculateValue();
        }
    }

}

