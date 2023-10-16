using UnityEngine;
using System;
using StatSystem;

namespace Vinci.Core.BattleFramework
{

    public class DamageableObject : MonoBehaviour
    {
        private StatSystem.Attribute _health;

        public StatSystem.Attribute Health { get => _health; set => _health = value; }

        public event Action<DamageableObject, float, Vector3> hit;
        public event Action<DamageableObject, float, Vector3> died;

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
        }

        public void takeDamage(int damage, Vector3 damagePoint, Vector3 hitNormal, IInfliectDamage attacker)
        {
            if (_health.currentValue <= 0)
            {
                Debug.Log("Is dead");
                return;
            }

            //Instantiate(playerImpactParticleSystem, damagePoint, Quaternion.LookRotation(hitNormal));

            _health.ApplyModifier(new SubtractModifier(damage));
            attacker.registerHit();

            if (_health.currentValue <= 0)
            {
                attacker.registerKill();
                died?.Invoke(this, damage, damagePoint);
            }
            else
            {

                hit?.Invoke(this, damage, damagePoint);
            }
        }
    }

}
