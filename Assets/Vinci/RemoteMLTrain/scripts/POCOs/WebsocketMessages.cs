using System;
using System.Collections.Generic;
using UnityEngine;

public enum MessagesID
{
    METRICS = 0,
    ACTIONS = 1,
    STATUS = 2,
    TRAIN_JOB_CONFIG = 3,
    ON_EPISODE_BEGIN = 4
}


[Serializable]
public class Header
{
    public int msg_id;
}

[Serializable]
public class MetricsMsg
{   
    public int msg_id;

    public string behaviour;
    public int step;
    public float time_elapsed;
    public float mean_reward;
    public float std_reward;

}

public class TrainJobStatusMsg
{
    public int msg_id;
    public string run_id;
    public TrainJobStatus status;
}

[Serializable]
public class ActionsHallwayMsg
{
    public int msg_id = (int)MessagesID.ACTIONS;

    public int stepCount = 0;
    public int episodeCount = 0;

    public int selection;
    public Pose agentPose;
    public Pose symbolOGoalPose;
    public Pose symbolXGoalPose;
    public Pose symbolOPose;
    public Pose symbolXPose;

    public List<int> actionsBuffer;
}

[Serializable]
public class ActionsRobotWaveMsg
{
    public int msg_id = (int)MessagesID.ACTIONS;

    public int stepCount = 0;
    public int episodeCount = 0;

    public Pose agentPose;
    public Pose symbolOGoalPose;
    public Pose symbolXGoalPose;
    public Pose symbolOPose;
    public Pose symbolXPose;

    public List<ActionRobotBufferMsg> actionsBuffer;
}

[Serializable]
public class ActionRobotBufferMsg
{
    public int direction;
    public int fire;
}

[Serializable]
public class TrainInfo
{
    public int stepCount = 0;
    public int episodeCount = 0;
}

[Serializable]
public class EpisodeBeginMsg
{
    public int msg_id = (int)MessagesID.ON_EPISODE_BEGIN;
}

[Serializable]
public class TrainJobMsg
{
    public int msg_id;
    public PostResponseTrainJob train_job;
}

[Serializable]
public class Pose
{
    public float x;
    public float y;
    public float z;
    public float xq;
    public float yq;
    public float zq;
    public float wq;

    public Pose(Vector3 position, Quaternion rotation)
    {
        x = position.x;
        y = position.y;
        z = position.z;
        xq = rotation.x;
        yq = rotation.y;
        zq = rotation.z;
        wq = rotation.w;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x, y, z);
    }
    
    public Quaternion GetRotation()
    {
        return new Quaternion(xq, yq, zq, wq);
    }
}

// return JsonUtility.FromJson<Actions>(json);