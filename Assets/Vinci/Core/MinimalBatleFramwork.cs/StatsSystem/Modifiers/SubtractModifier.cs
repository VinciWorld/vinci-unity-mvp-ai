using UnityEngine;

namespace StatSystem
{

    public class SubtractModifier : BaseModifier
    {

        public SubtractModifier(float magnitude) : base(magnitude, 0){}

        public override void Apply(ref float value)
        {
            value -= magnitude;
        }
    }
}
