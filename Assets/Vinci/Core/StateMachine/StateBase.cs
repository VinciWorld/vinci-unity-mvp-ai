
namespace Vinci.Core.StateMachine
{
    public abstract class StateBase
    {
        public abstract void OnEnterState();
        public abstract void Tick(float deltaTime);
        public abstract void OnExitState();

        public virtual void OneDrawGizmos() {}
    }
}

