using UnityEngine;

namespace StatSystem
{

    public class MultiplicationModifier : BaseModifier
    {
        private int _currentBaseValue;

        public MultiplicationModifier(float magnitude) : base(magnitude, 1){}

        public override void Apply(ref float value)
        {
           value +=  Mathf.RoundToInt(stat.baseValue * magnitude);
        }
    }
}
