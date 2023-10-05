using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Solana.Unity.SDK;
using UnityEngine;
using UnityEngine.UI;

public class BlockchainTest : MonoBehaviour
{

    public List<Image> nftsImages;


    void Start()
    {
        Byte[] model  = GetRawNNModel();
        Debug.Log(model.Length);
    }

    public Byte[] GetRawNNModel()
    {
        string path = Path.Combine(Application.persistentDataPath, "Hallway.onnx");

        byte[] rawModel = File.ReadAllBytes(path);

        return rawModel;
    }
}
