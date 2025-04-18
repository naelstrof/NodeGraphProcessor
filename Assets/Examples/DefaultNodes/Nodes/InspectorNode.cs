﻿using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/InspectorNode")]
public class InspectorNode : BaseNode {
    [Input(name = "In")] public float input;

    [Output(name = "Out")] public float output;

    [ShowInInspector] public bool additionalSettings;

    [ShowInInspector] public string additionalParam;

    public override string name => "InspectorNode";

    protected override void Process() {
        output = input * 42;
    }
}