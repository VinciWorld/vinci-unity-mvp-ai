using System.Collections.Generic;
using UnityEngine;

namespace Shared.Core.Utils
{
    public class TimedBehaviour : MonoBehaviour
    {

        readonly List<Timer> m_ActiveTimers = new List<Timer>();

        protected void StartTimer(Timer newTimer)
        {
            if (m_ActiveTimers.Contains(newTimer))
            {
                Debug.LogWarning("Timer already exists!");
            }
            else
            {
                m_ActiveTimers.Add(newTimer);
            }
        }

        protected void PauseTimer(Timer timer)
        {
            timer.isToRemove = true;



        }

        protected void StopTimer(Timer timer)
        {
            timer.Reset();
            PauseTimer(timer);
        }

        private int indexToRemove = -1;
        protected virtual void FixedUpdate()
        {   
            for (int i = m_ActiveTimers.Count - 1; i >= 0; i--)
            {   
                if(m_ActiveTimers[i].isToRemove)
                {
                    indexToRemove = i;

                }
                else if(m_ActiveTimers[i].UpdateTime(Time.deltaTime))
                {
                    StopTimer(m_ActiveTimers[i]);
                }
            }

            if (indexToRemove != -1 &&  m_ActiveTimers.Contains(m_ActiveTimers[indexToRemove]))
            {
                m_ActiveTimers.Remove(m_ActiveTimers[indexToRemove]);
                indexToRemove = -1;
            }
        }
    }
}