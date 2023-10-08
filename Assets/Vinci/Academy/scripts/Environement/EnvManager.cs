using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Vinci.Academy.Environement
{

    public class EnvManager : MonoBehaviour
    {
        [SerializeField]
        NavMeshSurface navMeshSurface;

        public EnvironementBase CreateTrainEnv(TrainEnvironmentConfig envConfig)
        {
            EnvironementBase envInstatiate = null;

            envInstatiate = Instantiate(envConfig.prefab, new Vector3(0, 0, 0), Quaternion.identity);

            navMeshSurface.BuildNavMesh();

            return envInstatiate;
        }

        public List<EnvironementBase> CreateMutipleTrainEnvs(
            TrainEnvironmentConfig envConfig, int numEvns = 1, float spacing = 5f)
        {
            float xPos = 0f;
            List<EnvironementBase> envsInstatiate = new List<EnvironementBase>();


            for (int i = 0; i < numEvns; i++)
            {
                envsInstatiate.Add(Instantiate(envConfig.prefab, new Vector3(xPos, 0, 0), Quaternion.identity));
                xPos += envConfig.bounds.extents.x + spacing;
            }

            navMeshSurface.BuildNavMesh();

            Debug.Log("Envs created: " + numEvns);

            return envsInstatiate;
        }

        //private TrainEnvironment GetEnvById(string envId)
        //{
        // return trainEnvironments.FirstOrDefault(env => env.id == envId);
        //}


    }
}