using System;
using UnityEngine;

namespace Vinci.Academy.Ml.Data
{
    [Serializable]
    public class TrainEnvironment
    {
        public string id;
        public string envName;

        public Bounds bounds;

        public GameObject prefab;
    }

}