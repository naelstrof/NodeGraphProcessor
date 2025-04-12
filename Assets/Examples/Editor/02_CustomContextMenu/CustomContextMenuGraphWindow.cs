using GraphProcessor;
using UnityEditor;
using UnityEngine;

public class CustomContextMenuGraphWindow : BaseGraphWindow {
    private BaseGraph tmpGraph;

    protected override void OnDestroy() {
        graphView?.Dispose();
        DestroyImmediate(tmpGraph);
    }

    [MenuItem("Window/02 Custom Context Menu")]
    public static BaseGraphWindow OpenWithTmpGraph() {
        var graphWindow = CreateWindow<CustomContextMenuGraphWindow>();

        // When the graph is opened from the window, we don't save the graph to disk
        graphWindow.tmpGraph = CreateInstance<BaseGraph>();
        graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
        graphWindow.InitializeGraph(graphWindow.tmpGraph);

        graphWindow.Show();

        return graphWindow;
    }

    protected override void InitializeWindow(BaseGraph graph) {
        titleContent = new GUIContent("Context Menu Graph");

        if (graphView == null) {
            graphView = new CustomContextMenuGraphView(this);
            graphView.Add(new MiniMapView(graphView));
        }

        rootView.Add(graphView);
    }
}