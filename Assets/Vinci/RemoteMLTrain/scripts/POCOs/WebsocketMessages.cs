using System;

public enum MessagesID
{
    METRICS = 0,
    ACTIONS = 1,
    STATUS = 2
}


[System.Serializable]
public class Header
{
    public int msg_id;
}

[System.Serializable]
public class Metrics
{   
    public int id;

    public string behaviour;
    public int Step;
    public float TimeElapsed;
    public float MeanReward;
    public float StdOfReward;

}

[System.Serializable]
public class Actions
{
    public int id;
    public string data;

}

// return JsonUtility.FromJson<Actions>(json);