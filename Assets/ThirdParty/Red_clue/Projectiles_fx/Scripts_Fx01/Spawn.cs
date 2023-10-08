using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject firepoint;
    public List<GameObject> fx = new List<GameObject>();
    public RotateToMouse rotate;


    private GameObject effectToSpawn;
    private float timeToFire = 0;
    private int number = 0;
    void Start()
    {
        effectToSpawn = fx[0];
    }


    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= timeToFire)
        {
            timeToFire = Time.time + 1 / effectToSpawn.GetComponent<movement>().fireRate;
            SpawnVFX();
        }


        if (Input.GetKeyDown(KeyCode.D))
            Next();

        if (Input.GetKeyDown(KeyCode.A))
            Previous();
    }

    void SpawnVFX()
    {
        GameObject fx;

        if (firepoint != null)
        {
            fx = Instantiate(effectToSpawn, firepoint.transform.position, Quaternion.identity);
            if (rotate != null)
            {
                fx.transform.localRotation = rotate.GetRotation();
            }
        }

        else
        {
            Debug.Log("No Fire Point");
        }
    }

    public void Next()
    {
        number++;

        if (number > fx.Count)
            number = 0;

        for (int i = 0; i < fx.Count; i++)
        {
            if (number == i) effectToSpawn = fx[i];
            
        }
    }

    public void Previous()
    {
        number--;

        if (number < 0)
            number = fx.Count;

        for (int i = 0; i < fx.Count; i++)
        {
            if (number == i) effectToSpawn = fx[i];
            
        }
    }

    }