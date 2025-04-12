using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(GraphBehaviour))]
public class GraphBehaviourEditor : Editor {
    private Editor graphEditor;
    private GraphBehaviour behaviour => target as GraphBehaviour;

    private void OnEnable() {
        graphEditor = CreateEditor(behaviour.graph);
    }

    private void OnDisable() {
        DestroyImmediate(graphEditor);
    }

    public override VisualElement CreateInspectorGUI() {
        var root = new VisualElement();
        var graphContainer = graphEditor != null ? graphEditor.CreateInspectorGUI().Q("ExposedParameters") : null;

        root.Add(new Button(() => EditorWindow.GetWindow<AllGraphWindow>().InitializeGraph(behaviour.graph)) {
            text = "Open"
        });

        root.Add(graphContainer);

        return root;
    }
}