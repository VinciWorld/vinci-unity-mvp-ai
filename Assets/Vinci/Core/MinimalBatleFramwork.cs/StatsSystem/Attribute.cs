using System;
using UnityEngine;

namespace StatSystem
{
    public class Attribute : Stat
    {
        public Attribute(ProgressionStat statProgression, StatController controller) 
        : base(statProgression, controller){}

        protected float _currentValue;
        public float currentValue => _currentValue;

        public event Action<float, float> currentValueChanged;
        public event Action<IModifier> appliedModifier;

        public override void Initialize(int level)
        {
            base.Initialize(level);
            _currentValue = _baseValue;
            baseValueChanged += OnTotalBaseValueChanged;

            currentValueChanged?.Invoke(_currentValue, baseValue); //update at start
        }

        public virtual bool ApplyModifier(IModifier modifier)
        {
            float lastCurrentValue = _currentValue;

            modifier.SetStat(this);
            modifier.Apply(ref _currentValue);

            _currentValue = Mathf.Clamp(_currentValue, 0, totalValue);

            appliedModifier?.Invoke(modifier);

            if(lastCurrentValue != _currentValue )
            {
                currentValueChanged?.Invoke(_currentValue, baseValue);
                return true;
            }

            return false;
        }

        public override void Reset()
        {
            base.Reset();
            _currentValue = _baseValue;
            currentValueChanged?.Invoke(_currentValue, baseValue);
        }

        //If modifiers are removed we shouldn't excced the current max capacity
        public void OnTotalBaseValueChanged(float modifiersValue, float currentTotalValue)
        {
            Mathf.Clamp(_currentValue, 0, totalValue);
        }
    }
}
