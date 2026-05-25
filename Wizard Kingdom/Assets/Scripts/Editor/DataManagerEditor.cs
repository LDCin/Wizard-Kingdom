using Managers;
using UnityEditor;
using UnityEngine;

namespace WizardKingdom.Editor
{
    [CustomEditor(typeof(DataManager))]
    public class DataManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            DataManager manager = (DataManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("User Data Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Load User Data Into Editor"))
            {
                manager.LoadEditorDataFromUserData();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Save Editor Data To User Data"))
            {
                manager.ApplyEditorDataToUserData();
                EditorUtility.SetDirty(manager);
            }

            if (GUILayout.Button("Save User Data File"))
            {
                manager.SaveUserData();
            }
        }
    }
}
