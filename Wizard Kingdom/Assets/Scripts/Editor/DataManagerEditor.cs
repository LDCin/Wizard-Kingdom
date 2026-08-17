using Managers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Select one DataManager to edit data.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Data Tools", EditorStyles.boldLabel);

        DataManager dataManager = (DataManager)target;

        if (GUILayout.Button("Save Data From Inspector"))
        {
            dataManager.ApplyEditorDataToUserData();
            EditorUtility.SetDirty(dataManager);
        }

        if (GUILayout.Button("Reset Data"))
        {
            if (EditorUtility.DisplayDialog(
                    "Reset Data",
                    "Reset user data to default values?",
                    "Reset",
                    "Cancel"))
            {
                dataManager.ResetData();
                EditorUtility.SetDirty(dataManager);
            }
        }
    }
}
