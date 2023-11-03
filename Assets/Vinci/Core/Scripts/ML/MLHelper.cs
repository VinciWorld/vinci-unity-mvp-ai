using System;
using System.IO;
using Unity.Sentis;
using Unity.Sentis.ONNX;
using Unity.VisualScripting;
using UnityEngine;

namespace Vinci.Core.ML.Utils
{
    public static class MLHelper
    {
        public static (string filePath, ModelAsset nnModel) SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName)
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

                ModelAsset nnModel = LoadModelRuntime(rawModel, behaviourName);
              
                return (filePath, nnModel);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ModelAsset LoadModelRuntime(byte[] rawModel, string behaviourName)
        {
            var asset = ScriptableObject.CreateInstance<ModelAsset>();
            asset.modelAssetData = ScriptableObject.CreateInstance<ModelAssetData>();
            asset.modelAssetData.value = rawModel;
            asset.name = behaviourName;

            return asset;

            /*
            Model assetData = ScriptableObject.CreateInstance<NNModelData>();
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                ModelWriter.Save(writer, onnxModel);
                assetData.Value = memoryStream.ToArray();
            }
            assetData.name = "Data";
            assetData.hideFlags = HideFlags.HideInHierarchy;
            

            var asset = ScriptableObject.CreateInstance<Model>();
            asset.modelData = assetData;

            asset.name = behaviourName;
            */
        }
    }
}

