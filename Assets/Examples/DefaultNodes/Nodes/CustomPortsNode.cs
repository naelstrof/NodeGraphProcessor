using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[Serializable]
[NodeMenuItem("Custom/MultiPorts")]
public class CustomPortsNode : BaseNode {
    [Input] public List<float> inputs;

    [Output] public List<float> outputs; // TODO: custom function for this one

    // We keep the max port count so it doesn't cause binding issues
    [SerializeField] [HideInInspector] private int portCount = 1;

    private List<object> values = new();

    public override string name => "CustomPorts";

    public override string layoutStyle => "TestType";

    protected override void Process() {
        // do things with values
    }

    [CustomPortBehavior(nameof(inputs))]
    private IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges) {
        portCount = Mathf.Max(portCount, edges.Count + 1);

        for (var i = 0; i < portCount; i++)
            yield return new PortData {
                displayName = "In " + i,
                displayType = typeof(float),
                identifier = i.ToString() // Must be unique
            };
    }

    // This function will be called once per port created from the `inputs` custom port function
    // will in parameter the list of the edges connected to this port
    [CustomPortInput(nameof(inputs), typeof(float))]
    private void PullInputs(List<SerializableEdge> inputEdges) {
        values.AddRange(inputEdges.Select(e => e.passThroughBuffer).ToList());
    }

    [CustomPortOutput(nameof(outputs), typeof(float))]
    private void PushOutputs(List<SerializableEdge> connectedEdges) {
        // Values length is supposed to match connected edges length
        for (var i = 0; i < connectedEdges.Count; i++)
            connectedEdges[i].passThroughBuffer = values[Mathf.Min(i, values.Count - 1)];

        // once the outputs are pushed, we don't need the inputs data anymore
        values.Clear();
    }
}