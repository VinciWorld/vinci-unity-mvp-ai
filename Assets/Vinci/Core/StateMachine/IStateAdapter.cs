using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vinci.Core.StateMachine
{
    public class StateAdapter
    {
        public virtual void OnEnterState()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void OnExitState()
        {
        }

        public virtual void OneDrawGizmos()
        {

        }
    }
}