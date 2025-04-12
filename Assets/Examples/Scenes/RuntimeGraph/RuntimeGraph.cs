using GraphProcessor;
using UnityEngine;

public class RuntimeGraph : MonoBehaviour {
    public BaseGraph graph;

    public GameObject assignedGameObject;

    private int i;
    public ProcessGraphProcessor processor;

    private void Start() {
        if (graph != null)
            processor = new ProcessGraphProcessor(graph);
    }

    private void Update() {
        if (graph != null) {
            graph.SetParameterValue("Input", (float)i++);
            graph.SetParameterValue("GameObject", assignedGameObject);
            processor.Run();
            Debug.Log("Output: " + graph.GetParameterValue("Output"));
        }
    }
}