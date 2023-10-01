using UnityEngine;
using BestHTTP;
using BestHTTP.WebSocket;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using Vinci.Core.Utils;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using System.IO;

public class RemoteTrainManager : PersistentSingleton<RemoteTrainManager> 
{
    private WebSocket _webSocket;

    [SerializeField]
    private string centralNode = "127.0.0.1:8000";
    [SerializeField]
    private string http_prefix = "http://";
    [SerializeField]
    private string websockt_prefix = "ws://";
    
    const string endpointTainJobs = "/api/v1/train-jobs";
    const string endpointWebsoctClientStream = "/ws/v1/client-stream";

    public event Action<MetricsMsg> metricsReceived;
    public event Action episodeBegin;
    public event Action<string> actionsReceived;
    public event Action<string> statusReceived;
    public event Action<PostResponseTrainJob> trainJobConfigReceived;


    async public Task<PostResponseTrainJob> StartRemoteTrainning(PostTrainJobRequest requestData)
    {
        string url = http_prefix + centralNode + endpointTainJobs;

        string json = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        HTTPResponse response = await SendHTTPPostRequestAsync(url, json);

        if (response.StatusCode == 200)
        {
            PostResponseTrainJob trainJobResponse = JsonConvert.DeserializeObject<PostResponseTrainJob>(response.DataAsText);
            
            return trainJobResponse;
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }

    async public Task<NNModel> DownloadNNModel(string runId)
    {
        string url = http_prefix + centralNode + endpointTainJobs + "/" + runId + "/nn-models";

        HTTPResponse response = await SendHTTPGetRequestAsync(url);

        if (response.StatusCode == 200)
        {
            return LoadOnnxModel(response.Data);
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }


    public void ConnectWebSocketToTrainInstance()
    {
        string url = websockt_prefix + centralNode + endpointWebsoctClientStream;

        InitializeWebSocket(url);
    }

    public void ConnectWebSocketCentralNodeClientStream()
    {
        string url = websockt_prefix + centralNode + endpointWebsoctClientStream;

        InitializeWebSocket(url);
    }

    public void SendWebSocketJson(string jsonData)
    {
        if (_webSocket != null && _webSocket.IsOpen)
        {
            _webSocket.Send(jsonData);
        }
    }

    private void InitializeWebSocket(string url)
    {
        _webSocket = new WebSocket(new Uri(url));
        _webSocket.OnOpen += OnWebsocktSocketOpen;
        _webSocket.OnMessage += OnWebsocketMessageReceived;
        //_webSocket.OnBinary += (WebSocket ws, byte[] data) => OnWebSocketBinaryReceived?.Invoke(ws, data);
        _webSocket.OnClosed += OnWebSocketClosed;
        _webSocket.OnError += OnWebSocketError;
        _webSocket.Open();
    }

    private void OnWebsocktSocketOpen(WebSocket webSocket)
    {
        Debug.Log("WebSocket is now Open!");
        _webSocket = webSocket;
    }

    private void OnWebsocketMessageReceived(WebSocket webSocket, string message)
    {
        Header header = JsonUtility.FromJson<Header>(message);
        switch (header.msg_id)
        {
            case (int)MessagesID.METRICS:
                MetricsMsg metrics = JsonConvert.DeserializeObject<MetricsMsg>(message);
                metricsReceived?.Invoke(metrics);
                Debug.Log("Metrics received: " + message);
                break;

            case (int)MessagesID.ACTIONS:
                actionsReceived?.Invoke(message);
                break;

            case (int)MessagesID.STATUS:
                statusReceived?.Invoke(message);
                break;

            case (int)MessagesID.TRAIN_JOB_CONFIG:
                PostResponseTrainJob trainJobConfig = JsonConvert.DeserializeObject<PostResponseTrainJob>(message);
                trainJobConfigReceived?.Invoke(trainJobConfig);
                break;

            case (int)MessagesID.ON_EPISODE_BEGIN:
                episodeBegin?.Invoke();
                break;

            default:
                Debug.LogWarning("Unknown msg_id received: " + header.msg_id);
                break;
        }
    }

    private void OnWebSocketClosed(WebSocket webSocket, UInt16 code, string message)
    {
        //TODO: retry websocket connection
        _webSocket = null;
        Debug.Log("WebSocket is now Closed!");
    }

    private void OnWebSocketError(WebSocket ws, string error)
    {
        Debug.Log("Websockt error: " + error);
        _webSocket = null;
    }

    private async Task<HTTPResponse> SendHTTPPostRequestAsync(string url, string jsonData)
    {
        var tcs = new TaskCompletionSource<HTTPResponse>();

        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Post, (req, resp) =>
        {
            if (req.Exception != null)
                tcs.SetException(req.Exception);
            else
                tcs.SetResult(resp);
        });

        request.AddHeader("Content-Type", "application/json");
        request.RawData = Encoding.UTF8.GetBytes(jsonData);
        request.Send();

        return await tcs.Task;
    }

    public async Task<HTTPResponse> SendHTTPGetRequestAsync(string url)
    {
        var tcs = new TaskCompletionSource<HTTPResponse>();

        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, resp) =>
        {
            if (req.Exception != null)
                tcs.SetException(req.Exception);
            else
                tcs.SetResult(resp);
        });

        request.Send();

        return await tcs.Task;
    }

    private void SendHTTPPostRequest(string url, string jsonData, OnRequestFinishedDelegate callback)
    {
        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Post, callback);
        request.AddHeader("Content-Type", "application/json");
        request.RawData = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.Send();
    }

    NNModel LoadOnnxModel(byte[] rawModel)
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

        return asset;
    }
}
