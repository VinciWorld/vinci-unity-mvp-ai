using System;
using UnityEngine;


namespace Shared.Core.Utils
{
    public class Timer
    {
        public Action _callback;
        public Action<float> _updateTime;
        float _time;
        float _currentTime;
        bool _countDown;

        public bool isToRemove;

        public float NormalizedProgress
        {
            get { return Mathf.Clamp(_currentTime / _time, 0f, 1f); }
        }

        public float CurrentTime => _currentTime;

        public Timer(float newTime, bool countdown = false, Action onElapsed = null, Action<float> updateTime = null)
        {
            _time = newTime;

            if (newTime <= 0)
            {
                _time = 0.1f;
            }

            _currentTime = 0f;
            if(countdown)
            {
                _currentTime = newTime;
            }
       
            _countDown = countdown;
            _callback += onElapsed;
            _updateTime += updateTime;
        }

        public bool UpdateTime(float deltaTime)
        {
            _currentTime = _countDown ? _currentTime - deltaTime : _currentTime + deltaTime;
          
            
            if (_currentTime >= _time || _currentTime <= 0)
            {
                _callback?.Invoke();
                return true;
            }

            _updateTime?.Invoke(_currentTime);

            return false;
        }

        public void Reset()
        {
            _currentTime = 0;
        }

        public void SetTime(float newTime)
        {
            _time = newTime;

            if (newTime <= 0)
            {
                _time = 0.1f;
            }
        }


        void DisplayTime(float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            //timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}