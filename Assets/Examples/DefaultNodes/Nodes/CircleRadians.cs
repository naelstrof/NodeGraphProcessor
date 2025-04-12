﻿using System;
using System.Collections.Generic;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/CircleRadians")]
public class CircleRadians : BaseNode {
    [Output(name = "In")] public List<float> outputRadians;

    public override string name => "CircleRadians";

    [CustomPortOutput(nameof(outputRadians), typeof(float))]
    public void PushOutputRadians(List<SerializableEdge> connectedEdges) {
        var i = 0;

        // outputRadians should match connectedEdges length, the list is generated by the editor function

        foreach (var edge in connectedEdges) {
            edge.passThroughBuffer = outputRadians[i];
            i++;
        }
    }
}