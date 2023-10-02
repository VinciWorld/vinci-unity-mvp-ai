using System;

public enum MessagesID
{
    METRICS = 0,
    ACTIONS = 1,
    STATUS = 2,
    TRAIN_JOB_CONFIG = 3,
    ON_EPISODE_BEGIN = 4
}


[System.Serializable]
public class Header
{
    public int msg_id;
}

[System.Serializable]
public class MetricsMsg
{   
    public int id;

    public string behaviour;
    public int Step;
    public float TimeElapsed;
    public float MeanReward;
    public float StdOfReward;

}

[System.Serializable]
public class ActionsHallwayMsg
{
    public int id = (int)MessagesID.ACTIONS;

    public int stepCount = 0;
    public int episodeCount = 0;

    public int dir;
}

[System.Serializable]
public class TrainInfo
{
    public int stepCount = 0;
    public int episodeCount = 0;
}

[System.Serializable]
public class EpisodeBeginMsg
{
    public int id = (int)MessagesID.ON_EPISODE_BEGIN;


}


// return JsonUtility.FromJson<Actions>(json);