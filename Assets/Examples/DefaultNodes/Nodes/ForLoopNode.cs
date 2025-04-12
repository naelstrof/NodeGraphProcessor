using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NodeGraphProcessor.Examples;

[Serializable]
[NodeMenuItem("Conditional/ForLoop")]
public class ForLoopNode : ConditionalNode {
    public int start;
    public int end = 10;

    [Output] public int index;

    [Output(name = "Loop Body")] public ConditionalLink loopBody;

    [Output(name = "Loop Completed")] public ConditionalLink loopCompleted;

    public override string name => "ForLoop";

    protected override void Process() {
        index++;
        // Implement all logic that affects the loop inner fields
    }

    public override IEnumerable<ConditionalNode> GetExecutedNodes() {
        throw new Exception("Do not use GetExecutedNoes in for loop to get it's dependencies");
    }

    public IEnumerable<ConditionalNode> GetExecutedNodesLoopBody() {
        // Return all the nodes connected to the executes port
        return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopBody))
            .GetEdges().Select(e => e.inputNode as ConditionalNode);
    }

    public IEnumerable<ConditionalNode> GetExecutedNodesLoopCompleted() {
        // Return all the nodes connected to the executes port
        return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopCompleted))
            .GetEdges().Select(e => e.inputNode as ConditionalNode);
    }
}