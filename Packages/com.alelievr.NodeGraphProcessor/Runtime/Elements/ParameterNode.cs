﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor {
[Serializable]
public class ParameterNode : BaseNode {
    // We serialize the GUID of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
    [SerializeField] [HideInInspector] public string parameterGUID;

    public ParameterAccessor accessor;
    [Input] public object input;

    [Output] public object output;

    public override string name => "Parameter";

    public ExposedParameter parameter { get; private set; }

    public event Action onParameterChanged;

    protected override void Enable() {
        // load the parameter
        LoadExposedParameter();

        graph.onExposedParameterModified += OnParamChanged;
        if (onParameterChanged != null)
            onParameterChanged?.Invoke();
    }

    private void LoadExposedParameter() {
        parameter = graph.GetExposedParameterFromGUID(parameterGUID);

        if (parameter == null) {
            Debug.Log("Property \"" + parameterGUID + "\" Can't be found !");

            // Delete this node as the property can't be found
            graph.RemoveNode(this);
            return;
        }

        output = parameter.value;
    }

    private void OnParamChanged(ExposedParameter modifiedParam) {
        if (parameter == modifiedParam) onParameterChanged?.Invoke();
    }

    [CustomPortBehavior(nameof(output))]
    private IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges) {
        if (accessor == ParameterAccessor.Get)
            yield return new PortData {
                identifier = "output",
                displayName = "Value",
                displayType = parameter == null ? typeof(object) : parameter.GetValueType(),
                acceptMultipleEdges = true
            };
    }

    [CustomPortBehavior(nameof(input))]
    private IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges) {
        if (accessor == ParameterAccessor.Set)
            yield return new PortData {
                identifier = "input",
                displayName = "Value",
                displayType = parameter == null ? typeof(object) : parameter.GetValueType()
            };
    }

    protected override void Process() {
#if UNITY_EDITOR // In the editor, an undo/redo can change the parameter instance in the graph, in this case the field in this class will point to the wrong parameter
        parameter = graph.GetExposedParameterFromGUID(parameterGUID);
#endif

        ClearMessages();
        if (parameter == null) {
            AddMessage($"Parameter not found: {parameterGUID}", NodeMessageType.Error);
            return;
        }

        if (accessor == ParameterAccessor.Get)
            output = parameter.value;
        else
            graph.UpdateExposedParameter(parameter.guid, input);
    }
}

public enum ParameterAccessor {
    Get,
    Set
}
}