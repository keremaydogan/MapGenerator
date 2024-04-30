using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapDecoratorData)), CanEditMultipleObjects]
public class MapDecoratorDataEditor : Editor
{
    bool showRegionMaxHeights = true;

    SerializedProperty regionMaxHeights;
    SerializedProperty prefabInfos;
    SerializedProperty grassAssetInfos;

    private void OnEnable()
    {
        regionMaxHeights = serializedObject.FindProperty("regionMaxHeights");
        prefabInfos = serializedObject.FindProperty("prefabInfos");
        grassAssetInfos = serializedObject.FindProperty("grassAssetInfos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        regionMaxHeights.arraySize = regionMaxHeights.arraySize;

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((MapDecoratorData)target), typeof(MapDecoratorData), false);
        GUI.enabled = true;

        showRegionMaxHeights = EditorGUILayout.Foldout(showRegionMaxHeights, new GUIContent("Region Max Heights"));

        if (showRegionMaxHeights)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(regionMaxHeights.GetArrayElementAtIndex(0), new GUIContent("  Region " + 0));
            GUI.enabled = true;

            for (int i = 1; i < regionMaxHeights.arraySize - 1; i++)
            {
                EditorGUILayout.PropertyField(regionMaxHeights.GetArrayElementAtIndex(i), new GUIContent("  Region " + i));
            }
        }

        EditorGUILayout.PropertyField(prefabInfos, true);

        serializedObject.ApplyModifiedProperties();
    }

}
