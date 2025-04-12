using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(IfNode))]
public class IfNodeView : BaseNodeView {
    public override void Enable() {
        hasSettings = true; // or base.Enable();
        var node = nodeTarget as IfNode;

        // Create your fields using node's variables and add them to the controlsContainer

        controlsContainer.Add(new Label($"Last Evaluation: {node.condition}"));
    }
}