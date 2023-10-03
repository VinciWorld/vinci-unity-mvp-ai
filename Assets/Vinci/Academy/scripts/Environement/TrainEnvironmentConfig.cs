using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vinci.Academy.Environement
{

    //TrainEnvironmentConfig<T> where T : EnvEvaluationMetrics, new()
    [Serializable]
    public class TrainEnvironmentConfig
    {
        public string env_id;
        public string envName;

        public int num_of_areas;

        public Bounds bounds;

        [System.NonSerialized]
        public EnvironementBase prefab;
    }


    [Serializable]
    public class EnvConfigAndMetricsEntry
    {
        public TrainEnvironmentConfig envConfig;
        public EnvEvaluationMetrics evaluationMetrics;
    }

    [Serializable]
    public class EnvEvaluationMetrics
    {
        public int goalCompletedCount = 0;
        public int goalFailedCount = 0;
        public float goalSuccessRate = 0.0f;
    }
}