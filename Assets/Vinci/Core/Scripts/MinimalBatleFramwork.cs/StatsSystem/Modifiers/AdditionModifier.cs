using UnityEngine;

namespace StatSystem
{

    public class AdditionModifier : BaseModifier
    {

        public AdditionModifier(float magnitude) : base(magnitude, 0){}

        public override void Apply(ref float value)
        {
            value += magnitude;
        }
    }
}
