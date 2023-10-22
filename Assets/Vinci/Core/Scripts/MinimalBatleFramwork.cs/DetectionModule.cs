using System;
using UnityEngine;

namespace Vinci.Core.BattleFramework
{
    public class DetectionModule : MonoBehaviour
    {
        [SerializeField]
        public Transform _detectionOriginPoint;

        [SerializeField]
        private float _detectionRange = 100f;
        [SerializeField]
        private float _attackRange = 20f;
        public float AttackRange { get => _attackRange; set => _attackRange = value; }

        [SerializeField]
        private float viewAngle = 80f;

        public Targetable target { get; set; }

        [SerializeField]
        private LayerMask thingsToHit;

        [SerializeField]
        private LayerMask targetsMask;

        public Action onDetectedTarget;
        public Action onAttackeRange;
        public Action onLostTarget;

        public bool IsTargetInAttackRange { get; private set; }
        public bool IsSeeingTarget { get; private set; }


        public void HandleTargetDetection()
        {
            UpdateTarget();

            if (target == null) return;

            CheckTargetVisibilityAndAttackRange();
        }

        private void UpdateTarget()
        {
            if (target == null)
            {
                target = FindClosestTarget();

                if (target)
                {
                    onDetectedTarget?.Invoke();
                }
            }
        }

        private Targetable FindClosestTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _detectionRange, targetsMask);
            float closestDistance = float.MaxValue;
            Collider closestTarget = null;

            foreach (var collider in colliders)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider;
                }
            }

            return closestTarget?.transform.gameObject.GetComponent<Targetable>();
        }

        private void CheckTargetVisibilityAndAttackRange()
        {
            float sqrDistance = (target.transform.position - _detectionOriginPoint.position).sqrMagnitude;
            float sqrDetectionRange = _detectionRange * _detectionRange;

            if (sqrDistance < sqrDetectionRange)
            {
                CheckVisibilityAndAttackRange(sqrDistance);
            }
            else
            {
                LostTarget();
            }
        }

        private void CheckVisibilityAndAttackRange(float sqrDistance)
        {
            float sqrAttackRange = _attackRange * _attackRange;

            //Debug.Log(sqrDistance < sqrAttackRange);
            if (sqrDistance < sqrAttackRange && CanSeeTarget(target.targetableTransform, viewAngle, _attackRange))
            {
                IsTargetInAttackRange = true;
                //Debug.Log("In attakak range");
                if (!IsSeeingTarget)
                {
                    //Debug.Log("In attakak range");
                    IsSeeingTarget = true;
                    onAttackeRange?.Invoke();
                }
            }
            else
            {
                IsSeeingTarget = false;
                IsTargetInAttackRange = false;
            }
        }

        private void LostTarget()
        {
            onLostTarget?.Invoke();
            target = null;
            IsSeeingTarget = false;
            IsTargetInAttackRange = false;
        }

        bool CanSeeTarget(Transform target, float viewAngle, float viewRange)
        {
            Vector3 toTarget = target.position - _detectionOriginPoint.position;
            if (Vector3.Angle(transform.forward, toTarget) > viewAngle)
            {
                return false;
            }

            if(Physics.Raycast(_detectionOriginPoint.position, toTarget, out RaycastHit hit, viewRange, thingsToHit))
            {
                if(hit.collider.gameObject.tag == "Player")
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }

}