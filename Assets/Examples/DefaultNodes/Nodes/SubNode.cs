using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Operations/Sub")]
public class SubNode : BaseNode {
    [Input(name = "A")] public float inputA;

    [Input(name = "B")] public float inputB;

    [Output(name = "Out")] public float output;

    public override string name => "Sub";

    protected override void Process() {
        output = inputA - inputB;
    }
}