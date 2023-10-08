using UnityEngine;

[CreateAssetMenu(fileName = "RobotWaveEnv", menuName = "RobotWaveEnv/WaveConfig", order = 0)]
public class Wave : ScriptableObject
{
    public EnemyAI enemyPrefab;
    public int amout;
    public float intervalBetweenSpawns;
}
