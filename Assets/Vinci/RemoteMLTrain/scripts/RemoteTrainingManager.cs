using UnityEngine;
using BestHTTP;
using BestHTTP.WebSocket;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using Vinci.Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class RemoteTrainManager : PersistentSingleton<RemoteTrainManager> 
{

    private WebSocket _webSocket;

    public bool isTestMode;
    public bool isSecureConnection = false;
    public bool isConnected => _webSocket != null;

    [SerializeField]
    private string _jwtToken;

    [SerializeField]
    private string centralNode = "127.0.0.1:8000";
    
    [SerializeField]
    private string http_prefix = "http://";
    [SerializeField]
    private string websockt_prefix = "ws://";

    const string endpointTainJobs = "/api/v1/train-jobs";
    const string endpointUserLogin = "/api/v1/user/unity-login";
    const string endpointUser = "/api/v1/user";
    const string endpointWebsoctClientStream = "/ws/v1/client-stream";
    const string endpointWebsoctServerStream = "/api/v1/train-instance-stream";

    public event Action websocketOpen;
    public event Action<byte[]> binaryDataReceived;
    public event Action<MetricsMsg> metricsReceived;
    public event Action episodeBegin;
    public event Action<string> actionsReceived;
    public event Action<TrainJobStatusMsg> statusReceived;
    public event Action<PostResponseTrainJob> trainJobConfigReceived;


    protected override void Awake()
    {
        base.Awake();

        if(isSecureConnection)
        {
            http_prefix = "https://";
            websockt_prefix = "wss://";
        }
        else
        {
            centralNode = "127.0.0.1:8000";
        }
    }



    async public Task<PostResponseTrainJob> StartRemoteTrainning(PostTrainJobRequest requestData)
    {
        string url = http_prefix + centralNode + endpointTainJobs;

        string json = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        Debug.Log(json);

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

    async public Task<byte[]> DownloadNNModel(string runId)
    {
        string url = http_prefix + centralNode + endpointTainJobs + "/" + runId + "/nn-model";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest(); // Send the HTTP GET request asynchronously

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.data;
            }
            else
            {
                Debug.LogError($"HTTP Request failed with status code {request.responseCode}: {request.error}");
                throw new Exception($"HTTP Request failed with status code {request.responseCode}: {request.error}");
            }
        }
    }

    async public void LoginCentralNode(UserUpdate userDataUpdate, Action<UserData> callback)
    {   
        string url = http_prefix + centralNode + endpointUserLogin;

        string json = JsonConvert.SerializeObject(userDataUpdate, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include // Include null values in the JSON
        });

        HTTPResponse response = await SendHTTPPostRequestAsync(url, json);

        if (response.StatusCode == 200)
        {
            UserData userDataResponse = JsonConvert.DeserializeObject<UserData>(response.DataAsText);
            Sprite userAvatar = null;
            try
            {
                Debug.Log(response.DataAsText);
                userAvatar = await DownloadImageAsync(userDataResponse.image_url);

                userDataResponse.avatar = userAvatar;
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to load user avatar: " + e.Message);
            }


            callback?.Invoke(userDataResponse);
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }

    async public Task<string> SaveOnArweaveModelFromS3(string run_id)
    {
        string url = http_prefix + "92dwrpewod.execute-api.eu-central-1.amazonaws.com/dev/blockchain/mutateArweaveUpload";
        RunIdArweave runIdArweve = new RunIdArweave();
        runIdArweve.runId = run_id;
        PostArweave postRunIdArweave = new PostArweave();
        postRunIdArweave.json = runIdArweve;

        string json = JsonConvert.SerializeObject(postRunIdArweave);

        HTTPResponse response = await SendHTTPPostRequestAsync(url, json, true);

        if (response.StatusCode == 200)
        {
            ResultUriArweave resultUriArweave = JsonConvert.DeserializeObject<ResultUriArweave>(response.DataAsText);
            Debug.Log("response.DataAsText" + response.DataAsText);

            return resultUriArweave.result.data.json;
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }

    public async Task<UserData> SaveUserPlayerDataAsync(string user_id, string playerData)
    {
        string url = http_prefix + centralNode + endpointUser;
        HTTPResponse response = await SendHTTPPatchRequestAsync(url, playerData);
   
        if (response.StatusCode == 200)
        {

            return JsonUtility.FromJson<UserData>(response.DataAsText);
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {response.StatusCode}: {response.Message}");
        }
    }
    //string url = http_prefix + centralNode + endpointTainJobs + $"/{runId}";
    public async Task<PostResponseTrainJob> GetTrainJobByRunID(string runId, Action<PostResponseTrainJob> callback)
    {
        string url = http_prefix + centralNode + endpointTainJobs + $"/{runId}";

        UnityWebRequest request = UnityWebRequest.Get(url);

        if (!isTestMode)
        {
            // Add the "Authorization" header with the JWT token when not in test mode
            request.SetRequestHeader("Authorization", "Bearer " + _jwtToken);
        }

        try{
            await request.SendWebRequest(); // Send the HTTP GET request asynchronously
        }
        catch(Exception e)
        {
            if (request.responseCode == 404)
            {
                throw new NotFoundException("Not found run id: " + runId + " error: " + e.Message);
            }
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            Debug.Log(jsonResult);
            PostResponseTrainJob trainJob = JsonConvert.DeserializeObject<PostResponseTrainJob>(jsonResult);

            //callback?.Invoke(trainJob);

            return trainJob;
        }
        else
        {
            throw new Exception($"HTTP Request failed with status code {request.responseCode}: {request.error}");
        }
    }
    public async Task<Sprite> DownloadImageAsync(string imageUrl)
    {

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        await www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("Error downloading image: " + www.error);
            return null;
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            return sprite;
        }
    }

    public void ConnectWebSocketToTrainInstance()
    {
        string url = websockt_prefix + centralNode + endpointWebsoctServerStream;

        InitializeWebSocket(url);
    }

    public void ConnectWebSocketCentralNodeClientStream()
    {
        string url = websockt_prefix + centralNode + endpointWebsoctClientStream;

        InitializeWebSocket(url);
    }

    public void CloseWebSocketConnection()
    {
        _webSocket.Close();
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
        _webSocket.OnBinary += OnWebSocketBinaryReceived;
        _webSocket.OnClosed += OnWebSocketClosed;
        _webSocket.OnError += OnWebSocketError;
        _webSocket.Open();
    }

    private void OnWebsocktSocketOpen(WebSocket webSocket)
    {
        Debug.Log("WebSocket is now Open!");
        websocketOpen?.Invoke();
        _webSocket = webSocket;
    }

    private void OnWebSocketBinaryReceived(WebSocket ws, byte[] data)
    {
        binaryDataReceived?.Invoke(data);
    }

    private void OnWebsocketMessageReceived(WebSocket webSocket, string message)
    {
        //Debug.Log(message);
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
                TrainJobStatusMsg trainStatus = JsonConvert.DeserializeObject<TrainJobStatusMsg>(message);
                statusReceived?.Invoke(trainStatus);
                break;

            case (int)MessagesID.TRAIN_JOB_CONFIG:
                TrainJobMsg trainJobConfig = JsonConvert.DeserializeObject<TrainJobMsg>(message);
                trainJobConfigReceived?.Invoke(trainJobConfig.train_job);
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
        Debug.Log("****WebSocket is now Closed!");
    }

    private void OnWebSocketError(WebSocket ws, string error)
    {
        Debug.Log("Websockt error: " + error);
        _webSocket = null;
    }

    private async Task<HTTPResponse> SendHTTPPostRequestAsync(string url, string jsonData=null, bool isAws=false)
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

        if (!isTestMode)
        {
            if(isAws)
            {
                request.AddHeader("X-Amz-Security-Token", _jwtToken);
            }
            else
            {
                request.AddHeader("Authorization", "Bearer " + _jwtToken);
            }
        }

        Debug.Log("JWT: " + _jwtToken);

        if(jsonData != null)
        {
            request.RawData = Encoding.UTF8.GetBytes(jsonData);
        }

        await request.Send();

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

        if (!isTestMode)
        {
            request.AddHeader("Authorization", "Bearer " + _jwtToken);
        }

        await request.Send();

        return await tcs.Task;
    }


    private async Task<HTTPResponse> SendHTTPPatchRequestAsync(string url, string jsonData)
    {
        var tcs = new TaskCompletionSource<HTTPResponse>();

        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Patch, (req, resp) =>
        {
            if (req.Exception != null)
                tcs.SetException(req.Exception);
            else
                tcs.SetResult(resp);
        });

        request.AddHeader("Content-Type", "application/json");

        if (!isTestMode)
        {
            request.AddHeader("Authorization", "Bearer " + _jwtToken);
        }

        request.RawData = Encoding.UTF8.GetBytes(jsonData);
        await request.Send();

        return await tcs.Task;
    }

    private void SendHTTPPostRequest(string url, string jsonData, OnRequestFinishedDelegate callback)
    {
        HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Post, callback);
        request.AddHeader("Content-Type", "application/json");
        request.RawData = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.Send();
    }

    public void SetJwtToken(string jwtToken)
    {
        _jwtToken = jwtToken;
    }    
}
