using System;

[Serializable]
public class MetricsData
{
    public int id;
    public string behaviour;
    public int step;
    public float time_elapsed;
    public float mean_reward;
    public float std_reward;
}