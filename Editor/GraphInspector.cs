using UnityEditor;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class GraphInspector : Editor {
    protected ExposedParameterFieldFactory exposedParameterFactory;
    protected BaseGraph graph;

    private VisualElement parameterContainer;
    protected VisualElement root;

    protected virtual void OnEnable() {
        graph = target as BaseGraph;
        graph.onExposedParameterListChanged += UpdateExposedParameters;
        graph.onExposedParameterModified += UpdateExposedParameters;
        if (exposedParameterFactory == null)
            exposedParameterFactory = new ExposedParameterFieldFactory(graph);
    }

    protected virtual void OnDisable() {
        graph.onExposedParameterListChanged -= UpdateExposedParameters;
        graph.onExposedParameterModified -= UpdateExposedParameters;
        exposedParameterFactory?.Dispose(); //  Graphs that created in GraphBehaviour sometimes gives null ref.
        exposedParameterFactory = null;
    }

    public sealed override VisualElement CreateInspectorGUI() {
        root = new VisualElement();
        CreateInspector();
        return root;
    }

    protected virtual void CreateInspector() {
        parameterContainer = new VisualElement {
            name = "ExposedParameters"
        };
        FillExposedParameters(parameterContainer);

        root.Add(parameterContainer);
    }

    protected void FillExposedParameters(VisualElement parameterContainer) {
        if (graph.exposedParameters.Count != 0)
            parameterContainer.Add(new Label("Exposed Parameters:"));

        foreach (var param in graph.exposedParameters) {
            if (param.settings.isHidden)
                continue;

            var field = exposedParameterFactory.GetParameterValueField(param, newValue => {
                param.value = newValue;
                serializedObject.ApplyModifiedProperties();
                graph.NotifyExposedParameterValueChanged(param);
            });
            parameterContainer.Add(field);
        }
    }

    private void UpdateExposedParameters(ExposedParameter param) {
        UpdateExposedParameters();
    }

    private void UpdateExposedParameters() {
        parameterContainer.Clear();
        FillExposedParameters(parameterContainer);
    }

    // Don't use ImGUI
    public sealed override void OnInspectorGUI() {
    }
}
}