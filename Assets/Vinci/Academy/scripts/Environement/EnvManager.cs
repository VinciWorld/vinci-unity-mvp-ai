using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Vinci.Academy.Environement
{

    public class EnvManager : MonoBehaviour
    {

        public EnvironementBase CreateTrainEnv(TrainEnvironmentConfig envConfig)
        {
            EnvironementBase envInstatiate = null;

            envInstatiate = Instantiate(envConfig.prefab, new Vector3(0, 0, 0), Quaternion.identity);

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

            return envsInstatiate;
        }

        //private TrainEnvironment GetEnvById(string envId)
        //{
        // return trainEnvironments.FirstOrDefault(env => env.id == envId);
        //}


    }
}