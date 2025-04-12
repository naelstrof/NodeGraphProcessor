using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GraphProcessor {
[Serializable]
public abstract class BaseGraphWindow : EditorWindow {
    [SerializeField] protected BaseGraph graph;

    private readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";
    protected BaseGraphView graphView;

    private bool reloadWorkaround;
    protected VisualElement rootView;

    public bool isGraphLoaded => graphView != null && graphView.graph != null;

    protected virtual void Update() {
        // Workaround for the Refresh option of the editor window:
        // When Refresh is clicked, OnEnable is called before the serialized data in the
        // editor window is deserialized, causing the graph view to not be loaded
        if (reloadWorkaround && graph != null) {
            LoadGraph();
            reloadWorkaround = false;
        }
    }

    /// <summary>
    ///     Called by Unity when the window is enabled / opened
    /// </summary>
    protected virtual void OnEnable() {
        InitializeRootView();

        if (graph != null)
            LoadGraph();
        else
            reloadWorkaround = true;
    }

    /// <summary>
    ///     Called by Unity when the window is disabled (happens on domain reload)
    /// </summary>
    protected virtual void OnDisable() {
        if (graph != null && graphView != null)
            graphView.SaveGraphToDisk();
    }

    /// <summary>
    ///     Called by Unity when the window is closed
    /// </summary>
    protected virtual void OnDestroy() {
    }

    public event Action<BaseGraph> graphLoaded;
    public event Action<BaseGraph> graphUnloaded;

    private void LoadGraph() {
        // We wait for the graph to be initialized
        if (graph.isEnabled)
            InitializeGraph(graph);
        else
            graph.onEnabled += () => InitializeGraph(graph);
    }

    private void InitializeRootView() {
        rootView = rootVisualElement;

        rootView.name = "graphRootView";

        rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
    }

    public void InitializeGraph(BaseGraph graph) {
        if (this.graph != null && graph != this.graph) {
            // Save the graph to the disk
            EditorUtility.SetDirty(this.graph);
            AssetDatabase.SaveAssets();
            // Unload the graph
            graphUnloaded?.Invoke(this.graph);
        }

        graphLoaded?.Invoke(graph);
        this.graph = graph;

        if (graphView != null)
            rootView.Remove(graphView);

        //Initialize will provide the BaseGraphView
        InitializeWindow(graph);

        graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

        if (graphView == null) {
            Debug.LogError("GraphView has not been added to the BaseGraph root view !");
            return;
        }

        graphView.Initialize(graph);

        InitializeGraphView(graphView);

        // TOOD: onSceneLinked...

        if (graph.IsLinkedToScene())
            LinkGraphWindowToScene(graph.GetLinkedScene());
        else
            graph.onSceneLinked += LinkGraphWindowToScene;
    }

    private void LinkGraphWindowToScene(Scene scene) {
        EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

        void CloseWindowWhenSceneIsClosed(Scene closedScene) {
            if (scene == closedScene) {
                Close();
                EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
            }
        }
    }

    public virtual void OnGraphDeleted() {
        if (graph != null && graphView != null)
            rootView.Remove(graphView);

        graphView = null;
    }

    protected abstract void InitializeWindow(BaseGraph graph);

    protected virtual void InitializeGraphView(BaseGraphView view) {
    }
}
}