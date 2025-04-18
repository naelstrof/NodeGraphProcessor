﻿using GraphProcessor;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(BaseGraph), true)]
public class GraphAssetInspector : GraphInspector {
    // protected override void CreateInspector()
    // {
    // }

    protected override void CreateInspector() {
        base.CreateInspector();

        root.Add(new Button(() => EditorWindow.GetWindow<DefaultGraphWindow>().InitializeGraph(target as BaseGraph)) {
            text = "Open base graph window"
        });
        root.Add(new Button(() =>
            EditorWindow.GetWindow<CustomContextMenuGraphWindow>().InitializeGraph(target as BaseGraph)) {
            text = "Open custom context menu graph window"
        });
        root.Add(new Button(() =>
            EditorWindow.GetWindow<CustomToolbarGraphWindow>().InitializeGraph(target as BaseGraph)) {
            text = "Open custom toolbar graph window"
        });
        root.Add(new Button(() =>
            EditorWindow.GetWindow<ExposedPropertiesGraphWindow>().InitializeGraph(target as BaseGraph)) {
            text = "Open exposed properties graph window"
        });
    }
}