// Editor: Ovan741852
// right click on prefab, CustomTools/Prefab/PrefabTracker
// show those asset which include the right clicked prefab
// SearchTypes define the search target types
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class PrefabTracker
{
    [MenuItem("Assets/CustomTools/Prefab/PrefabTracker")]
    public static void Track()
    {
        EditorWindow.GetWindowWithRect<PrefabTrackerWindow>(new Rect(0, 0, 350, 500));
    }

    [MenuItem("Assets/CustomTools/Prefab/PrefabTracker", true)]
    private static bool TargetCheck()
    {
        var target = Selection.activeObject;
        var targetType = target.GetType();
        if (!targetType.Equals(typeof(GameObject)))
            return false;
        var assetPath = AssetDatabase.GetAssetPath(target);
        if (string.IsNullOrEmpty(assetPath))
            return false;
        return true;
    }

    public static Dictionary<string, Queue<string>> FindTypeFiles(Type[] types)
    {
        var typeMapping = new Dictionary<string, Queue<string>>();
        foreach (var type in types)
        {
            var key = type.Name;
            var files = AssetDatabase.FindAssets($"t:{type.Name}").
                    Select(g => AssetDatabase.GUIDToAssetPath(g));
            var value = new Queue<string>();
            foreach (var file in files)
                value.Enqueue(file);
            typeMapping[key] = value;
        }

        return typeMapping;
    }
}

public class PrefabTrackerWindow : EditorWindow
{
    public static readonly Type[] SearchTypes = new Type[] {
        typeof(GameObject),
        typeof(SceneAsset),
        typeof(ScriptableObject),
    };

    public static readonly string[] TypeNames =
        SearchTypes.Select(t => t.Name).ToArray();

    private string _targetGuid;
    private Object _target;
    private Dictionary<string, List<Object>> _resultMapping;

    private Vector2 _position;
    private Dictionary<string, bool> _foldOutStates = 
        new Dictionary<string, bool>();

    private void OnEnable()
    {
        _target = Selection.activeObject;
        var assetPath = AssetDatabase.GetAssetPath(_target);
        _targetGuid = AssetDatabase.AssetPathToGUID(assetPath);

        _foldOutStates.Clear();
        foreach (var type in SearchTypes)
            _foldOutStates[type.Name] = false;

        var sourceMapping = PrefabTracker.FindTypeFiles(SearchTypes);
        var sourceCount = sourceMapping.Sum(q => q.Value.Count);

        _resultMapping = new Dictionary<string, List<Object>>();
        int count = 0;
        EditorUtility.DisplayProgressBar("Searching..", $"0/{sourceCount}", 0);
        foreach (var pair in sourceMapping)
        {
            var mapping = pair.Value;
            var list = new List<Object>();
            _resultMapping[pair.Key] = list;
            while (mapping.Count != 0)
            {
                string line;
                bool foundGuid = false;
                var file = mapping.Dequeue();
                using (var reader = new StreamReader(file))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.Contains(_targetGuid))
                            continue;
                        foundGuid = true;
                        break;
                    }
                }

                if (foundGuid)
                    list.Add(AssetDatabase.LoadAssetAtPath<Object>(file));
                count++;

                EditorUtility.DisplayProgressBar(
                    "Searching..",
                    $"{count}/{sourceCount}",
                    (float)count / sourceCount);

                Thread.Sleep(10);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    private void OnGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField(_target.name, _target, typeof(GameObject), false);
        GUI.enabled = true;

        _position = EditorGUILayout.BeginScrollView(_position);
        foreach(var type in SearchTypes)
            DrawTypeResult(type);
        EditorGUILayout.EndScrollView();
    }

    private void DrawTypeResult(Type type)
    {
        var typeName = type.Name;
        var foldOut = _foldOutStates[typeName];
        var mapping = _resultMapping[typeName];
        foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(foldOut, $"{typeName}({mapping.Count})");
        if (foldOut)
        {
            GUI.enabled = false;
            var identLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = identLevel + 1;
            foreach (var obj in mapping)
                EditorGUILayout.ObjectField(obj.name, obj, type, false);
            EditorGUI.indentLevel = identLevel;
            GUI.enabled = true;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        _foldOutStates[typeName] = foldOut;
    }
}
