using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vinci.Academy.Ml.Data;

public class EnvManager : MonoBehaviour
{

    public GameObject CreateTrainEnv(TrainEnvironment env, int numEvns = 1, float spacing = 5f)
    {
        float xPos = 0f;
        GameObject envInstatiate = null;


        for (int i = 0; i < numEvns; i++)
        {
            envInstatiate =  GameObject.Instantiate(env.prefab);

            xPos += env.bounds.extents.x;
        }

        Debug.Log(envInstatiate.transform.position);

        return envInstatiate;
    }

    //private TrainEnvironment GetEnvById(string envId)
    //{
       // return trainEnvironments.FirstOrDefault(env => env.id == envId);
    //}


}
