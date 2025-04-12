using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/MultiAdd")]
public class MultiAddNode : BaseNode {
    [Output] public float output;

    [Input] public IEnumerable<float> inputs;

    public override string name => "Add";

    protected override void Process() {
        output = 0;

        if (inputs == null)
            return;

        foreach (var input in inputs)
            output += input;
    }

    [CustomPortBehavior(nameof(inputs))]
    private IEnumerable<PortData> GetPortsForInputs(List<SerializableEdge> edges) {
        yield return new PortData { displayName = "In ", displayType = typeof(float), acceptMultipleEdges = true };
    }

    [CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
    public void GetInputs(List<SerializableEdge> edges) {
        inputs = edges.Select(e => (float)e.passThroughBuffer);
    }
}