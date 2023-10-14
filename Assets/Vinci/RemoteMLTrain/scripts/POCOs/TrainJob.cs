using System;
using Newtonsoft.Json;

[Serializable]
[JsonConverter(typeof(TrainJobStatusConverter))]
public enum TrainJobStatus
{
    SUBMITTED,
    RETRIEVED,
    STARTING,
    RUNNING,
    SUCCEEDED,
    FAILED
}


[Serializable]
public enum TrainJobType
{
    CREATE,
    RESUME
}


[Serializable]
public class PostResponseTrainJob
{
    public string run_id;
    public string agent_config;
    public EnvConfigSmall env_config;
    public BehaviorConfigSmall nn_model_config;
    public TrainJobStatus job_status;
    public TrainJobType job_type;
    public DateTime created_at;
}


[Serializable]
public class BehaviorConfigSmall
{
    public string behavior_name;
    public int steps;

}


[Serializable]
public class EnvConfigSmall
{
    public string agent_id;
    public string env_id;
    public int num_of_areas;
}


[Serializable]
public class RunId
{
    public string run_id { get; set; }
}


#nullable enable

[Serializable]
public class PostTrainJobRequest
{
    public string? run_id;
    public string? agent_config;
    public EnvConfigSmall? env_config;
    public BehaviorConfigSmall? nn_model_config;
}


