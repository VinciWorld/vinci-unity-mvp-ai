using System;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using UnityEngine;

namespace Vinci.Core.ML.Utils
{
    public static class MLHelper
    {

        public static (string filePath, NNModel nnModel) SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName)
        {
            try
            {
                string directoryPath = Path.Combine(Application.persistentDataPath, "runs", runId, "models");
                string filePath = Path.Combine(directoryPath, $"{behaviourName}.onnx");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                //Save model to disk
                File.WriteAllBytes(filePath, rawModel);
                Debug.Log("Model saved at: " + filePath);

                NNModel nnModel = MLHelper.LoadModelRuntime(behaviourName, rawModel);
                nnModel.name = behaviourName;

                return (filePath, nnModel);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static NNModel LoadModelRuntime(string behaviourName, byte[] rawModel)
        {
            var converter = new ONNXModelConverter(true);
            var onnxModel = converter.Convert(rawModel);

            NNModelData assetData = ScriptableObject.CreateInstance<NNModelData>();
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                ModelWriter.Save(writer, onnxModel);
                assetData.Value = memoryStream.ToArray();
            }
            assetData.name = "Data";
            assetData.hideFlags = HideFlags.HideInHierarchy;

            var asset = ScriptableObject.CreateInstance<NNModel>();
            asset.modelData = assetData;

            asset.name = behaviourName;

            return asset;
        }
    }
}

