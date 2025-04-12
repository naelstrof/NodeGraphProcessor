using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class GraphAssetCallbacks {
    [MenuItem("Assets/Create/GraphProcessor", false, 10)]
    public static void CreateGraphPorcessor() {
        var graph = ScriptableObject.CreateInstance<BaseGraph>();
        ProjectWindowUtil.CreateAsset(graph, "GraphProcessor.asset");
    }

    [OnOpenAsset(0)]
    public static bool OnBaseGraphOpened(int instanceID, int line) {
        var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

        if (asset != null && AssetDatabase.GetAssetPath(asset).Contains("Examples")) {
            EditorWindow.GetWindow<AllGraphWindow>().InitializeGraph(asset);
            return true;
        }

        return false;
    }
}