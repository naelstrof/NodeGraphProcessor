using GraphProcessor;
using UnityEditor;
using UnityEngine;

public class DefaultGraphWindow : BaseGraphWindow {
    private BaseGraph tmpGraph;

    protected override void OnDestroy() {
        graphView?.Dispose();
        DestroyImmediate(tmpGraph);
    }

    [MenuItem("Window/01 DefaultGraph")]
    public static BaseGraphWindow OpenWithTmpGraph() {
        var graphWindow = CreateWindow<DefaultGraphWindow>();

        // When the graph is opened from the window, we don't save the graph to disk
        graphWindow.tmpGraph = CreateInstance<BaseGraph>();
        graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
        graphWindow.InitializeGraph(graphWindow.tmpGraph);

        graphWindow.Show();

        return graphWindow;
    }

    protected override void InitializeWindow(BaseGraph graph) {
        titleContent = new GUIContent("Default Graph");

        if (graphView == null)
            graphView = new BaseGraphView(this);

        rootView.Add(graphView);
    }
}