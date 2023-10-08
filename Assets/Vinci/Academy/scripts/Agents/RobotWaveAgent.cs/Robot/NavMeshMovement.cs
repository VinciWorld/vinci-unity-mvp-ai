using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    private NavMeshAgent _navAgent;
    private Transform _target;

    private float _refreshRate = 0.25f;

    [Tooltip("The speed at which the enemy rotates")]
    public float _orientationSpeed = 10f;

    // Start is called before the first frame update
    void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_target != null && _navAgent != null)
        {
            _navAgent.SetDestination(_target.position);
            OrientTowards(_target.position);
        }
    }

    public void SetPosition(Vector3 position)
    {
        _navAgent.Warp(position);
        _navAgent.enabled = true;
    }

    public void SetNavDestination(Transform target)
    {
        _target = target;

        if (_navAgent && target != null)
        {
            _navAgent.SetDestination(target.position);
        }
    }

    public Vector3 GetAgentSpeed()
    {
        return _navAgent.velocity;
    }

    public void SetSpeed(int speed)
    {
        _navAgent.speed = speed;
    }

    public void OrientTowards(Vector3 lookPosition)
    {
        Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
        if (lookDirection.sqrMagnitude != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _orientationSpeed);
        }
    }

    //Can't use due to ml agents
    IEnumerator UpdatePath()
    {
        while (_target != null)
        {
            _navAgent.SetDestination(_target.position);
            yield return new WaitForSeconds(_refreshRate);
        }
    }

}
