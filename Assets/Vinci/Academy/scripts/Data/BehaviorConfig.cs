using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorConfig", menuName = "Academy/BehaviorConfig", order = 1)]
public class BehaviorConfig : ScriptableObject
{
    public string trainer_type = "ppo";
    public int max_steps = 500000;
    public int time_horizon = 64;
    public int summary_freq = 10000;
    public int keep_checkpoints = 5;
    public int checkpoint_interval = 50000;
    public bool threaded = false;
    public string init_path = null;
    public Hyperparameters hyperparameters = new Hyperparameters();
    public NetworkSettings network_settings = new NetworkSettings();

    public bool useMemory = false;
    public Memory memory = new Memory();

    public bool useBehavioralCloning = false;
    public BehavioralCloning behavioral_cloning = new BehavioralCloning();

    public bool useSelfPlay = false;
    public SelfPlay self_play = new SelfPlay();

    public List<RewardSignalEntry> reward_signals = new List<RewardSignalEntry>();

    public bool ShouldSerializebehavioral_cloning() => useBehavioralCloning;
    public bool ShouldSerializeself_play() => useSelfPlay;
    public bool ShouldSerializememory() => useMemory;

    public string SerializeToJson()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Converters.Add(new RewardSignalConverter());
        string json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
        Debug.Log(json);
        return json;
    }
}


[Serializable]
public class Hyperparameters
{
    public int batch_size = 1024;
    public int buffer_size = 10240;
    public float learning_rate = 3.0e-4f;
    public string learning_rate_schedule = "linear";
    public float beta = 5.0e-3f;
    public string beta_schedule = "constant";
    public float epsilon = 0.2f;
    public string epsilon_schedule = "linear";
    public float lambd = 0.95f;
    public int num_epoch = 3;
    public bool shared_critic = false;
}

[Serializable]
public class NetworkSettings
{
    public string vis_encode_type = "simple";
    public bool normalize = false;
    public int hidden_units = 128;
    public int num_layers = 2;
}

[Serializable]
public class Memory
{
    public int sequence_length = 64;
    public int memory_size = 256;
}

[Serializable]
public class BehavioralCloning
{
    public string demo_path = "Project/Assets/ML-Agents/Examples/Pyramids/Demos/ExpertPyramid.demo";
    public float strength = 0.5f;
    public int steps = 150000;
    public int batch_size = 512;
    public int num_epoch = 3;
    public int samples_per_update = 0;
}

[Serializable]
public class SelfPlay
{
    public int window = 10;
    public float play_against_latest_model_ratio = 0.5f;
    public int save_steps = 50000;
    public int swap_steps = 2000;
    public int team_change = 100000;
}

[Serializable]
public class RewardSignal
{
    public float strength = 1.0f;
    public float gamma = 0.99f;
}

public enum RewardSignalType
{
    Extrinsic,
    Curiosity,
    Gail
}

[Serializable]
public class RewardSignalEntry
{
    public RewardSignalType type;
    public RewardSignal rewardSignal;
}

[Serializable]
public class ExtrinsicReward : RewardSignal { }

[Serializable]
public class CuriosityReward : RewardSignal
{
    public int encoding_size = 256;
    public float learning_rate = 3.0e-4f;
}

[Serializable]
public class GailReward : RewardSignal
{
    public int encoding_size = 128;
    public string demo_path;
    public float learning_rate = 3.0e-4f;
    public bool use_actions = false;
    public bool use_vail = false;
}


public class RewardSignalConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(RewardSignal).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var type = value.GetType();
        JObject jo = JObject.FromObject(value, serializer);
        jo.Add("rewardType", type.Name);
        jo.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string typeName = jo["rewardType"].Value<string>();
        Type type = Type.GetType(typeName);
        return jo.ToObject(type, serializer);
    }
}