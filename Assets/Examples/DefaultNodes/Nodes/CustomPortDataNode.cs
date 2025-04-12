using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[Serializable]
[NodeMenuItem("Custom/PortData")]
public class CustomPortData : BaseNode {
    private static PortData[] portDatas = {
        new() { displayName = "0", displayType = typeof(float), identifier = "0" },
        new() { displayName = "1", displayType = typeof(int), identifier = "1" },
        new() { displayName = "2", displayType = typeof(GameObject), identifier = "2" },
        new() { displayName = "3", displayType = typeof(Texture2D), identifier = "3" }
    };

    [Output] public float output;

    [Input(name = "In Values", allowMultiple = true)]
    public IEnumerable<object> inputs = null;

    public override string name => "Port Data";

    protected override void Process() {
        output = 0;

        if (inputs == null)
            return;

        foreach (float input in inputs)
            output += input;
    }

    [CustomPortBehavior(nameof(inputs))]
    private IEnumerable<PortData> GetPortsForInputs(List<SerializableEdge> edges) {
        var pd = new PortData();

        foreach (var portData in portDatas) yield return portData;
    }

    [CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
    public void GetInputs(List<SerializableEdge> edges) {
        // inputs = edges.Select(e => (float)e.passThroughBuffer);
    }
}