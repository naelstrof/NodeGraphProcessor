using System;
using UnityEngine;

namespace GraphProcessor {
[Serializable]
public class SerializableEdge : ISerializationCallbackReceiver {
    public string GUID;

    [SerializeField] private BaseGraph owner;

    [SerializeField] private string inputNodeGUID;
    [SerializeField] private string outputNodeGUID;

    public string inputFieldName;
    public string outputFieldName;

    // Use to store the id of the field that generate multiple ports
    public string inputPortIdentifier;
    public string outputPortIdentifier;

    [NonSerialized] public BaseNode inputNode;

    [NonSerialized] public NodePort inputPort;

    [NonSerialized] public BaseNode outputNode;
    [NonSerialized] public NodePort outputPort;

    //temporary object used to send port to port data when a custom input/output function is used.
    [NonSerialized] public object passThroughBuffer;

    public void OnBeforeSerialize() {
        if (outputNode == null || inputNode == null)
            return;

        outputNodeGUID = outputNode.GUID;
        inputNodeGUID = inputNode.GUID;
    }

    public void OnAfterDeserialize() {
    }

    public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort) {
        var edge = new SerializableEdge();

        edge.owner = graph;
        edge.GUID = Guid.NewGuid().ToString();
        edge.inputNode = inputPort.owner;
        edge.inputFieldName = inputPort.fieldName;
        edge.outputNode = outputPort.owner;
        edge.outputFieldName = outputPort.fieldName;
        edge.inputPort = inputPort;
        edge.outputPort = outputPort;
        edge.inputPortIdentifier = inputPort.portData.identifier;
        edge.outputPortIdentifier = outputPort.portData.identifier;

        return edge;
    }

    //here our owner have been deserialized
    public void Deserialize() {
        if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
            return;

        outputNode = owner.nodesPerGUID[outputNodeGUID];
        inputNode = owner.nodesPerGUID[inputNodeGUID];
        inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
        outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
    }

    public override string ToString() {
        return $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
    }
}
}