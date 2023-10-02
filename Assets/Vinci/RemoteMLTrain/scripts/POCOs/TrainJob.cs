using System;

public enum TrainJobStatus
{
    SUBMITTED,
    RETRIEVED,
    STARTING,
    RUNNING,
    SUCCEEDED,
    FAILED
}

public enum TrainJobType
{
    CREATE,
    RESUME
}

public class PostResponseTrainJob
{
    public string run_id;
    public string agent_config;
    public TrainJobEnvConfig env_config;
    public TrainJobNNModelConfig nn_model_config;
    public TrainJobStatus job_status;
    public TrainJobType job_type;
    public DateTime created_at;
}

public class TrainJobNNModelConfig
{
    public int steps;
}

public class TrainJobEnvConfig
{
    public string env_id;
    public int num_of_areas;
}


#nullable enable
public class PostTrainJobRequest
{
    public string? run_id;
    public string? agent_config;
    public TrainJobEnvConfig? env_config;
    public TrainJobNNModelConfig? nn_model_config;
}



