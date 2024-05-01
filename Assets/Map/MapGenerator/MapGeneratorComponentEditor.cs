using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

[CustomEditor(typeof(MapGeneratorComponent)), CanEditMultipleObjects]
public class MapGeneratorComponentEditor : Editor
{

    SerializedProperty terraGen;
    SerializedProperty pathGen;
    SerializedProperty mapDecorator;
    SerializedProperty generateMap;
    SerializedProperty useRandomSeed;
    SerializedProperty seed;
    SerializedProperty groundLayer;

    private void OnEnable()
    {
        terraGen = serializedObject.FindProperty("terraGen");
        pathGen = serializedObject.FindProperty("pathGen");
        mapDecorator = serializedObject.FindProperty("mapDecorator");
        generateMap = serializedObject.FindProperty("generateMap");
        useRandomSeed = serializedObject.FindProperty("useRandomSeed");
        seed = serializedObject.FindProperty("seed");
        groundLayer = serializedObject.FindProperty("groundLayer");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(terraGen, true);
        EditorGUILayout.PropertyField(pathGen, true);
        EditorGUILayout.PropertyField(mapDecorator, true);

        if (GUILayout.Button("Generate Map", GUILayout.Width(120))) {
            generateMap.boolValue = true;
        }

        EditorGUILayout.PropertyField(useRandomSeed, true);
        EditorGUILayout.PropertyField(seed, true);
        EditorGUILayout.PropertyField(groundLayer, true);

        serializedObject.ApplyModifiedProperties();
    }

}
/*

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
*/