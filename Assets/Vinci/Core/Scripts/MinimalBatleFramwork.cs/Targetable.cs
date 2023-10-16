using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vinci.Core.BattleFramework
{
    public enum Team
    {
        NEUTRAL,
        PLAYER,
        ENEMY,
    }

    public struct Metrics
    {
        public int missedAttaks;
        public int hits;
        public int kills;

    }

    public class Targetable : DamageableObject, IInfliectDamage
    {
        public Team team;

        [Tooltip("Trans that will be targeted")]
        public Transform targetTransform;

        protected Vector3 _currentPosition, _previousPosition;
        public Vector3 velocity { get; protected set; }

        //Metrics
        public Metrics metrics;


        public event Action hitTarget;
        public event Action killedTarget;
        public event Action missedTarget;

        public Transform targetableTransform
        {
            get
            {
                return targetTransform == null ? transform : targetTransform;
            }
        }

        public Vector3 position
        {
            get { return targetableTransform.position; }
        }

        public void GetTargtable()
        {
            throw new System.NotImplementedException();
        }

        protected override void Awake()
        {
            base.Awake();
            ResetPositionData();
        }

        //Resets the position, to calculate the velocity
        protected void ResetPositionData()
        {
            _currentPosition = this.transform.position;
            _previousPosition = this.transform.position;
        }

        //Calculates the velocity
        void FixedUpdate()
        {
            _currentPosition = position;
            velocity = (_currentPosition - _previousPosition) / Time.fixedDeltaTime;
            _previousPosition = _currentPosition;
        }

        public void registerHit()
        {
            metrics.hits++;
            hitTarget?.Invoke();
        }

        public void registerKill()
        {
            metrics.kills++;
            killedTarget?.Invoke();
        }

        public void registerMiss()
        {
            metrics.missedAttaks++;
            missedTarget?.Invoke();
        }
    }
}

