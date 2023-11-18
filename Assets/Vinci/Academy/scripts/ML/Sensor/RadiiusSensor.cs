using System;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RadiiusSensor : MonoBehaviour 
{
    public Vector3 mapOrigin = Vector3.zero;

    //Sensor
    public int maxDetectedObjects = 10;
    public float detectionRadius = 10f;
    public Vector3 mapSize; // Set this based on your map's dimensions
    public string[] detectableTags = new string[] { "Enemy", "Obstacle", "Projectile" };

    private float maxDistance;

    [SerializeField]
    private LayerMask _detectableLayers;

    void Start()
    {
        maxDistance = CalculateMaxDistance(mapSize);
    }

    public List<ObjectData> DetectObjects()
    {
        List<ObjectData> detectedObjects = new List<ObjectData>();
        Dictionary<Vector2, ObjectData> closestProjectiles = new Dictionary<Vector2, ObjectData>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, _detectableLayers);

        foreach (var hitCollider in hitColliders)
        {
            GameObject obj = hitCollider.gameObject;
            Vector3 directionToObject = (obj.transform.position - transform.position).normalized;
            Vector2 direction2D = new Vector2(directionToObject.x, directionToObject.z).normalized;
            float squaredDistance = (transform.position - obj.transform.position).sqrMagnitude;
            float normalizedDistance = NormalizeSquaredDistance(squaredDistance);
            int[] oneHotTag = OneHotEncodeTag(obj.tag);

            Vector2 normalizedPosition = NormalizePosition(new Vector2(obj.transform.position.x, obj.transform.position.z));

            if (obj.tag == "Projectile")
            {
                if (!closestProjectiles.ContainsKey(direction2D) || closestProjectiles[direction2D].normalizedDistance > normalizedDistance)
                {
                    closestProjectiles[direction2D] = new ObjectData(direction2D, normalizedDistance, normalizedPosition, oneHotTag);
                }
            }
            else
            {
                detectedObjects.Add(new ObjectData(direction2D, normalizedDistance, normalizedPosition, oneHotTag));
            }
        }

        // Add the closest projectiles in each direction
        foreach (var projectile in closestProjectiles.Values)
        {
            detectedObjects.Add(projectile);
        }

        // Sort detected objects by distance (normalizedDistance)
        detectedObjects.Sort((a, b) => a.normalizedDistance.CompareTo(b.normalizedDistance));

        return detectedObjects;
    }

    public void AddObservationsToSensor(VectorSensor sensor)
    {
        var detectedObjects = DetectObjects();
        int detectedCount = 0;

        foreach (var objData in detectedObjects)
        {
            if (detectedCount >= maxDetectedObjects) break;

            sensor.AddObservation(objData.direction);
            sensor.AddObservation(objData.normalizedDistance);

            foreach (var t in objData.oneHotTag)
            {
                sensor.AddObservation(t);
            }

            detectedCount++;
        }

        // Padding the remaining observations
        int paddingSize = (maxDetectedObjects - detectedCount) * (2 + 1 + detectableTags.Length);
        for (int i = 0; i < paddingSize; i++)
        {
            sensor.AddObservation(0f);
        }
    }

    public Vector2 NormalizePosition(Vector2 position)
    {
        Vector2 mapSize2D = new Vector2(mapSize.x, mapSize.z);


        return new Vector2(
            (position.x + mapOrigin.x) / mapSize2D.x,
            (position.y + mapOrigin.y) / mapSize2D.y
        );
    }

    int[] OneHotEncodeTag(string tag)
    {
        int[] oneHotVector = new int[detectableTags.Length];
        int index = System.Array.IndexOf(detectableTags, tag);
        if (index >= 0)
        {
            oneHotVector[index] = 1;
        }
        return oneHotVector;
    }

    public int GetObservationSize()
    {
        // Assuming each object will have 2 position, 2 (direction) + 1 (distance) + detectableTags.Length (one-hot tag)
        return maxDetectedObjects * (5 + detectableTags.Length);
    }

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

    public struct ObjectData
    {
        public Vector2 direction;
        public float normalizedDistance;
        public Vector2 normalizedPosition;
        public int[] oneHotTag;

        public ObjectData(Vector2 dir, float dist, Vector2 normPos, int[] tag)
        {
            direction = dir;
            normalizedDistance = dist;
            normalizedPosition = normPos;
            oneHotTag = tag;
        }
    }
}