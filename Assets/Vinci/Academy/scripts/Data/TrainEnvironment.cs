using System;
using UnityEngine;

namespace Vinci.Academy.Ml.Data
{
    [Serializable]
    public class TrainEnvironmentConfig
    {
        public string env_id;
        public string envName;

        public int num_of_areas;

        public Bounds bounds;
    
        public GameObject prefab;
    }

}