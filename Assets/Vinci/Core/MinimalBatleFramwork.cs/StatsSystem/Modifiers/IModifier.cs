using System;

namespace StatSystem
{
public interface IModifier : IComparable
    {
        void Apply(ref float value);
        void Remove();
        void SetStat(Stat stat);
    }
}
