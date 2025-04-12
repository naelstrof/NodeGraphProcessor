using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/MessageNode2")]
public class MessageNode2 : BaseNode {
    [Input(name = "In")] public float input;

    [Output(name = "Out")] public float output;

    public override string name => "MessageNode2";

    protected override void Process() {
        output = input * 42;
    }
}