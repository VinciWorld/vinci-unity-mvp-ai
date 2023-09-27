using UnityEngine;
using BestHTTP;
using BestHTTP.WebSocket;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;

public class RemoteTrainingManager : MonoBehaviour 
{
    private WebSocket _webSocket;

    public delegate void WebSocketMessageReceived(WebSocket webSocket, string message);
    public event WebSocketMessageReceived OnWebSocketMessageReceived;

    public delegate void WebSocketBinaryReceived(WebSocket webSocket, byte[] data);
    public event WebSocketBinaryReceived OnWebSocketBinaryReceived;

    public delegate void WebSocketOpened(WebSocket webSocket);
    public event WebSocketOpened OnWebSocketOpened;

    public delegate void WebSocketClosed(WebSocket webSocket, UInt16 code, string message);
    public event WebSocketClosed OnWebSocketClosed;

    public delegate void WebSocketError(WebSocket webSocket, string error);
    public event WebSocketError OnWebSocketError;

    const string websockt_prefix = "ws://";
    const string centralNode = "127.0.0.1:8000";

    const string endpointPostTrainJob = "/ws/v1/client-stream";
    const string endpointWebsoctClientStream = "/ws/v1/client-stream";
    


    public void ConnectWebSocketTotrainNodeFromServer()
    {
        string url = websockt_prefix + centralNode + endpointPostTrainJob;

        InitializeWebSocket(url);
    }

    async public Task<TrainJobResponse> SendTrainJob(TrainJobRequest requestData)
    {
        string url = websockt_prefix + centralNode + endpointPostTrainJob;

        string json = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        HTTPResponse response = await SendHTTPPostRequestAsync(url, json);

        if (response.StatusCode == 200)
        {
            TrainJobResponse trainJobResponse = JsonConvert.DeserializeObject<TrainJobResponse>(response.DataAsText);
            
            return trainJobResponse;
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }

    private void OnWebsocktSocketOpen(WebSocket webSocket)
    {
        Debug.Log("WebSocket is now Open!");
        _webSocket = webSocket;
    }

    private void SendWebSocketJson(string jsonData)
    {
        if (_webSocket != null && _webSocket.IsOpen)
        {
            _webSocket.Send(jsonData);
        }
    }




    private void InitializeWebSocket(string url)
    {
        _webSocket = new WebSocket(new Uri(url));
        _webSocket.OnOpen += (WebSocket ws) => OnWebSocketOpened?.Invoke(ws);
        _webSocket.OnMessage += (WebSocket ws, string message) => OnWebSocketMessageReceived?.Invoke(ws, message);
        _webSocket.OnBinary += (WebSocket ws, byte[] data) => OnWebSocketBinaryReceived?.Invoke(ws, data);
        _webSocket.OnClosed += (WebSocket ws, UInt16 code, string message) => OnWebSocketClosed?.Invoke(ws, code, message);
        _webSocket.OnError += (WebSocket ws, string error) => OnWebSocketError?.Invoke(ws, error);
        _webSocket.Open();
    }

    public async Task<HTTPResponse> SendHTTPPostRequestAsync(string url, string jsonData)
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

    private void SendHTTPPostRequest(string url, string jsonData, OnRequestFinishedDelegate callback)
    {
        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Post, callback);
        request.AddHeader("Content-Type", "application/json");
        request.RawData = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.Send();
    }
}

