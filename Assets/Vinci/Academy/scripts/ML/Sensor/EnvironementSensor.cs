using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnvironementSensor : MonoBehaviour 
{    
    [SerializeField]
    EnemyManager _enemyManager;

    [SerializeField]
    LayerMask targetLayerMask;


    //Obsrvations
    [SerializeField]
    float proximityThreshold = 3.1f;
    [SerializeField]
    float errorThreshold = 0.992f;

    float closeErrorThreshold = 0.93f;
    float farErrorThreshold = 0.998f;
    float maxDistanceBeyondThreshold = 20f;

    public Vector3 mapSize; // Set this based on your map's dimensions
    private float maxDistance;


    float NormalizeSquaredDistance(float squaredDistance)
    {
        float maxSquaredDistance = maxDistance * maxDistance;
        return Mathf.Clamp01(squaredDistance / maxSquaredDistance);
    }

    float NormalizeDistance(float distance)
    {
        return Mathf.Clamp01(distance / maxDistance);
    }

    float CalculateMaxDistance(Vector3 size)
    {
        return size.magnitude;
    }

    #region old

    public bool IsPlayerLookingAtClosestEnemy(Transform player)
    {
        EnemyAI enemyAI = GetClosestEnemy();

        if(enemyAI != null)
        {
            return IsPlayerAimingAtEnemyRay(player);
        }

        return false;
    }


    private bool IsPlayerAimingAtEnemyRay(Transform player)
    {
        Ray aimingRay = new Ray(new Vector3(player.position.x, 1f, player.position.z), player.forward);
        RaycastHit hit;

        //Debug.Log(player.forward);

        if (Physics.Raycast(aimingRay, out hit, Mathf.Infinity, targetLayerMask))
        {
            if (hit.collider)
            {
                Debug.DrawLine(player.position, hit.point, Color.green, 0.1f);
                return true;
            }
        }
        return false;
    }


    private float IsPlayerLookingAtEnemy(Transform player, Transform enemy)
    {
        Vector3 directionToEnemy = enemy.position - player.position;
        float distanceToEnemy = directionToEnemy.magnitude;

        float dotProductValue = Vector3.Dot(player.forward, directionToEnemy.normalized);

        // If enemy is close and in front of the player, return true
        Debug.Log(distanceToEnemy);
        if (distanceToEnemy <= proximityThreshold && dotProductValue > 0.9)
        {
            Debug.DrawLine(player.position, enemy.position, Color.red, 0.5f);
            return 1;
        }

        float adjustedErrorThreshold = Mathf.Lerp(closeErrorThreshold, farErrorThreshold, (distanceToEnemy - proximityThreshold) / maxDistanceBeyondThreshold);
        // If enemy is further away, use the stricter dot product check based on errorThreshold
        if (dotProductValue >= adjustedErrorThreshold)
        {
            Debug.DrawLine(player.position, enemy.position, Color.green, 0.5f);
            return 1;
        }

        return dotProductValue;
    }

    public EnemyAI GetClosestEnemy()
    {
        return GetClosestEnemy(_enemyManager.allEnemies);
    }


    public EnemyAI GetClosestEnemy(List<EnemyAI> enemies)
    {
        EnemyAI closestEnemy = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) 
            {
                continue; 
            }

            float distanceSqr = (transform.position - enemies[i].transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemies[i];
            }
        }

        return closestEnemy;
    }
#endregion
}