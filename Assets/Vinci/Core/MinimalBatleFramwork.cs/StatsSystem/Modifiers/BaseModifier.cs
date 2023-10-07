

namespace StatSystem
{
    public abstract class BaseModifier : IModifier
    {
        protected Stat stat;
        protected readonly float magnitude;
        protected readonly int priority;

        public BaseModifier(float magnitude, int priority)
        {
            this.priority = priority;
            this.magnitude = magnitude;
        }

        public void SetStat(Stat stat)
        {
            this.stat = stat;
        }

        public abstract void Apply(ref float value);

        public virtual void Remove()
        {
            stat.RemoveModifier(this);
        }

        public int CompareTo(object obj)
        {
            int otherPriority = ((BaseModifier)obj).priority;
            return priority > otherPriority ? 1 : priority < otherPriority ? -1 : 0;
        }
    }
}

