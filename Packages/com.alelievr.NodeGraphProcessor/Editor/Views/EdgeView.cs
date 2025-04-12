using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class EdgeView : Edge {
    private readonly string edgeStyle = "GraphProcessorStyles/EdgeView";
    public bool isConnected = false;

    public EdgeView() {
        styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
        RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    public SerializableEdge serializedEdge => userData as SerializableEdge;

    protected BaseGraphView owner => ((input ?? output) as PortView).owner.owner;

    public override void OnPortChanged(bool isInput) {
        base.OnPortChanged(isInput);
        UpdateEdgeSize();
    }

    public void UpdateEdgeSize() {
        if (input == null && output == null)
            return;

        var inputPortData = (input as PortView)?.portData;
        var outputPortData = (output as PortView)?.portData;

        for (var i = 1; i < 20; i++)
            RemoveFromClassList($"edge_{i}");
        var maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
        if (maxPortSize > 0)
            AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
    }

    protected override void OnCustomStyleResolved(ICustomStyle styles) {
        base.OnCustomStyleResolved(styles);

        UpdateEdgeControl();
    }

    private void OnMouseDown(MouseDownEvent e) {
        if (e.clickCount == 2) {
            // Empirical offset:
            var position = e.mousePosition;
            position += new Vector2(-10f, -28);
            var mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

            owner.AddRelayNode(input as PortView, output as PortView, mousePos);
        }
    }
}
}