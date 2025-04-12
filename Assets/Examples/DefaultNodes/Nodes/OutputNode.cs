using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/OutputNode")]
public class OutputNode : BaseNode {
    [Input(name = "In")] public float input;

    public override string name => "OutputNode";

    public override bool deletable => false;

    protected override void Process() {
        // Do stuff
    }
}