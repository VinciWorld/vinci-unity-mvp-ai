using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;
using Unity.Sentis.ONNX;
[assembly: InternalsVisibleTo("Unity.Sentis.EditorTests")]
[assembly: InternalsVisibleTo("Unity.Sentis.Tests")]


namespace Unity.Sentis
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
        
                    var converter = new ONNXModelConverter(true);
                    Model sentisModel = converter.Convert(filePath, directoryPath);
                    //Model sentisModel = ModelLoader.Load(filePath);

                    ModelWriter.Save(filePath, sentisModel);
                    Debug.Log("Model saved at: " + filePath);

                    Byte[] rawSentisModel = File.ReadAllBytes(filePath);

                    //MemoryStream descStream = Open(rawSentisModel);
                    //int modelVersion = Read<int>(descStream);

                    //Debug.Log("modelVersion: " + modelVersion);

                    ModelAsset nnModel = LoadModelRuntime(rawSentisModel, sentisModel, behaviourName);

                    return (filePath, nnModel);


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ModelAsset LoadModelRuntime(string behaviourName, string modelPath)
        {
            byte[] rawModel = File.ReadAllBytes(modelPath);
            Model sentisModel = ModelLoader.Load(modelPath);

            var asset = ScriptableObject.CreateInstance<ModelAsset>();

            asset.modelAssetData = ScriptableObject.CreateInstance<ModelAssetData>();
            asset.modelAssetData.name = "Data";
            asset.modelAssetData.hideFlags = HideFlags.HideInHierarchy;
            asset.modelAssetData.value = rawModel;

            var weightStreams = new List<MemoryStream>();
            SaveModelWeights(weightStreams, sentisModel);

            asset.modelWeightsChunks = new ModelAssetWeightsData[weightStreams.Count];
            for (int i = 0; i < weightStreams.Count; i++)
            {
                var stream = weightStreams[i];
                asset.modelWeightsChunks[i] = ScriptableObject.CreateInstance<ModelAssetWeightsData>();
                asset.modelWeightsChunks[i].value = stream.ToArray();
                asset.modelWeightsChunks[i].name = "Data";
                asset.modelWeightsChunks[i].hideFlags = HideFlags.HideInHierarchy;

                stream.Close();
                stream.Dispose();
            }

            asset.name = behaviourName;

            return asset;
        }

        public static ModelAsset LoadModelRuntime(Byte[] rawModel, Model sentisModel, string behaviourName)
        {

            var asset = ScriptableObject.CreateInstance<ModelAsset>();

            asset.modelAssetData = ScriptableObject.CreateInstance<ModelAssetData>();
            asset.modelAssetData.name = "Data";
            asset.modelAssetData.hideFlags = HideFlags.HideInHierarchy;
            asset.modelAssetData.value = rawModel;

            var weightStreams = new List<MemoryStream>();
            SaveModelWeights(weightStreams, sentisModel);

            asset.modelWeightsChunks = new ModelAssetWeightsData[weightStreams.Count];
            for (int i = 0; i < weightStreams.Count; i++)
            {
                var stream = weightStreams[i];
                asset.modelWeightsChunks[i] = ScriptableObject.CreateInstance<ModelAssetWeightsData>();
                asset.modelWeightsChunks[i].value = stream.ToArray();
                asset.modelWeightsChunks[i].name = "Data";
                asset.modelWeightsChunks[i].hideFlags = HideFlags.HideInHierarchy;

                stream.Close();
                stream.Dispose();
            }

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

        static MemoryStream Open(byte[] bytes)
        {
            return new MemoryStream(bytes, 0, bytes.Length, false, true);
        }

        static T Read<T>(Stream stream) where T : unmanaged
        {
            unsafe
            {
                Span<byte> arr = stackalloc byte[sizeof(T)];
                stream.Read(arr);
                T dst = default(T);
                fixed (byte* src = &arr[0])
                {
                    Buffer.MemoryCopy(src, &dst, sizeof(T), sizeof(T));
                }
                return dst;
            }
        }

        public static void SaveModelWeights(List<MemoryStream> streams, Model model)
        {

            streams.Clear();
            streams.Add(new MemoryStream());

            int streamIndex = 0;
            MemoryStream stream = streams[streamIndex];

            long writtenLength = 0;
            // write constant data
            for (var l = 0; l < model.constants.Count; ++l)
            {
                var constant = model.constants[l];
                if (constant.weights == null)
                    continue;
                var sizeOfDataItem = constant.weights.SizeOfType;
                int memorySize = constant.length * sizeOfDataItem;
                if (writtenLength + (long)memorySize >= (long)Int32.MaxValue)
                {
                    streams.Add(new MemoryStream());
                    streamIndex++;
                    stream = streams[streamIndex];
                    writtenLength = 0;
                }
                writtenLength += memorySize;
                unsafe
                {
                    var src = model.constants[l].weights.AddressAt<byte>(constant.offset * sizeOfDataItem);
                    Span<byte> span = new Span<byte>(src, memorySize);
                    stream.Write(span);
                }
            }

        }
    }
}

