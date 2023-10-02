using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vinci.Academy.Ml.Data;

public class EnvManager : MonoBehaviour
{

    public GameObject CreateTrainEnv(TrainEnvironmentConfig envConfig)
    {
        GameObject envInstatiate = null;

        envInstatiate =  Instantiate(envConfig.prefab, new Vector3(0, 0, 0), Quaternion.identity);

        return envInstatiate;
    }

    public List<GameObject> CreateMutipleTrainEnvs(
        TrainEnvironmentConfig envConfig, int numEvns = 1, float spacing = 5f)
    {
        float xPos = 0f;
        List<GameObject> envsInstatiate = new List<GameObject>();


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
