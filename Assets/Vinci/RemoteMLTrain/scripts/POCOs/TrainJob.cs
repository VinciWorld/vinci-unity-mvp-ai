

public enum TrainJobStatus
{
    SUBMITTED,
    RETRIEVED,
    STARTING,
    RUNNING,
    SUCCEEDED,
    FAILED
}

public class PostResponseTrainJob
{
    public string run_id;
    public string agent_config;
    public string env_config;
    public string nn_model_config;
    public TrainJobStatus status;
}

#nullable enable
public class PostTrainJobRequest
{
    public string? run_id;
    public string? agent_config;
    public string? env_config;
    public string? nn_model_config;
}

