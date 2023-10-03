using System;
using UnityEngine;

public abstract class EnvironementBase : MonoBehaviour
{
    public event Action OnUpdateResults;

    public abstract HallwayAgent GetAgent();

    public abstract void Initialize(HallwayAgent agent);
    public abstract void EpisodeBegin();
    public abstract void GoalCompleted(bool result);
}