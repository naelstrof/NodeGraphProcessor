using System;
using GraphProcessor;

[Serializable]
[NodeMenuItem("Custom/MessageNode")]
public class MessageNode : BaseNode {
    private const string k_InputIsNot42Error = "Input is not 42 !";

    [Input(name = "In")] public float input;

    [Setting("Message Type")] public NodeMessageType messageType = NodeMessageType.Error;

    public override string name => "MessageNode";

    protected override void Process() {
        if (input != 42)
            AddMessage(k_InputIsNot42Error, messageType);
        else
            RemoveMessage(k_InputIsNot42Error);
    }
}