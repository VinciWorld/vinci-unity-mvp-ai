#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(BehaviorConfigSmall))]
public class BehaviorConfigEditor : Editor
{
    SerializedProperty rewardSignalsProperty;

    private void OnEnable()
    {
        rewardSignalsProperty = serializedObject.FindProperty("reward_signals");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector properties
        DrawPropertiesExcluding(serializedObject, "reward_signals");

        // Handle RewardSignalEntry list
        EditorGUILayout.LabelField("Reward Signals", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        BehaviorConfig settings = (BehaviorConfig)target;

        for (int i = 0; i < settings.reward_signals.Count; i++)
        {
            var entry = settings.reward_signals[i];

            entry.type = (RewardSignalType)EditorGUILayout.EnumPopup("Type", entry.type);

            switch (entry.type)
            {
                case RewardSignalType.Extrinsic:
                    if (!(entry.rewardSignal is ExtrinsicReward))
                    {
                        entry.rewardSignal = new ExtrinsicReward();
                    }
                    break;
                case RewardSignalType.Curiosity:
                    if (!(entry.rewardSignal is CuriosityReward))
                    {
                        entry.rewardSignal = new CuriosityReward();
                    }
                    break;
                case RewardSignalType.Gail:
                    if (!(entry.rewardSignal is GailReward))
                    {
                        entry.rewardSignal = new GailReward();
                    }
                    break;
            }

            // Manually draw the fields of the RewardSignal object
            var rewardSignal = entry.rewardSignal;
            rewardSignal.strength = EditorGUILayout.FloatField("Strength", rewardSignal.strength);
            rewardSignal.gamma = EditorGUILayout.FloatField("Gamma", rewardSignal.gamma);

            if (rewardSignal is CuriosityReward curiosityReward)
            {
                curiosityReward.encoding_size = EditorGUILayout.IntField("Encoding Size", curiosityReward.encoding_size);
                curiosityReward.learning_rate = EditorGUILayout.FloatField("Learning Rate", curiosityReward.learning_rate);
            }
            else if (rewardSignal is GailReward gailReward)
            {
                gailReward.encoding_size = EditorGUILayout.IntField("Encoding Size", gailReward.encoding_size);
                gailReward.demo_path = EditorGUILayout.TextField("Demo Path", gailReward.demo_path);
                gailReward.learning_rate = EditorGUILayout.FloatField("Learning Rate", gailReward.learning_rate);
                gailReward.use_actions = EditorGUILayout.Toggle("Use Actions", gailReward.use_actions);
                gailReward.use_vail = EditorGUILayout.Toggle("Use VAIL", gailReward.use_vail);
            }
        }

        if (GUILayout.Button("Add Reward Signal"))
        {
            settings.reward_signals.Add(new RewardSignalEntry());
        }

        EditorGUI.indentLevel--;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif