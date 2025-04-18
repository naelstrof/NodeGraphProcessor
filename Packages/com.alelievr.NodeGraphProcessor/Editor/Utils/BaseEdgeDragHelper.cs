using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class BaseEdgeDragHelper : EdgeDragHelper {
    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Manipulators/EdgeDragHelper.cs#L21
    internal const int k_PanAreaWidth = 30;
    internal const int k_PanSpeed = 4;
    internal const int k_PanInterval = 10;
    internal const float k_MinSpeedFactor = 0.5f;
    internal const float k_MaxSpeedFactor = 7f;
    internal const float k_MaxPanSpeed = k_MaxSpeedFactor * k_PanSpeed;
    internal const float kPortDetectionWidth = 30;
    protected static NodeAdapter nodeAdapter = new();
    protected readonly IEdgeConnectorListener listener;

    protected Dictionary<BaseNodeView, List<PortView>> compatiblePorts = new();
    private Edge ghostEdge;
    protected GraphView graphView;

    private Vector2 lastMousePos;
    private Vector3 panDiff = Vector3.zero;

    private IVisualElementScheduledItem panSchedule;
    private bool wasPanned;

    public BaseEdgeDragHelper(IEdgeConnectorListener listener) {
        this.listener = listener;
        resetPositionOnPan = true;
        Reset();
    }

    public bool resetPositionOnPan { get; set; }

    public override Edge edgeCandidate { get; set; }

    public override Port draggedPort { get; set; }

    public override void Reset(bool didConnect = false) {
        if (compatiblePorts != null && graphView != null) {
            // Reset the highlights.
            graphView.ports.ForEach(p => { p.OnStopEdgeDragging(); });
            compatiblePorts.Clear();
        }

        // Clean up ghost edge.
        if (ghostEdge != null && graphView != null) {
            var pv = ghostEdge.input as PortView;
            graphView.schedule.Execute(() => {
                pv.portCapLit = false;
                // pv.UpdatePortView(pv.portData);
            }).ExecuteLater(10);
            graphView.RemoveElement(ghostEdge);
        }

        if (wasPanned)
            if (!resetPositionOnPan || didConnect) {
                var p = graphView.contentViewContainer.transform.position;
                var s = graphView.contentViewContainer.transform.scale;
                graphView.UpdateViewTransform(p, s);
            }

        if (panSchedule != null)
            panSchedule.Pause();

        if (ghostEdge != null) {
            ghostEdge.input = null;
            ghostEdge.output = null;
        }

        if (draggedPort != null && !didConnect) {
            draggedPort.portCapLit = false;
            draggedPort = null;
        }

        if (edgeCandidate != null) edgeCandidate.SetEnabled(true);

        ghostEdge = null;
        edgeCandidate = null;

        graphView = null;
    }

    public override bool HandleMouseDown(MouseDownEvent evt) {
        var mousePosition = evt.mousePosition;

        if (draggedPort == null || edgeCandidate == null) return false;

        graphView = draggedPort.GetFirstAncestorOfType<GraphView>();

        if (graphView == null) return false;

        if (edgeCandidate.parent == null) graphView.AddElement(edgeCandidate);

        var startFromOutput = draggedPort.direction == Direction.Output;

        edgeCandidate.candidatePosition = mousePosition;
        edgeCandidate.SetEnabled(false);

        if (startFromOutput) {
            edgeCandidate.output = draggedPort;
            edgeCandidate.input = null;
        } else {
            edgeCandidate.output = null;
            edgeCandidate.input = draggedPort;
        }

        draggedPort.portCapLit = true;

        compatiblePorts.Clear();

        foreach (PortView port in graphView.GetCompatiblePorts(draggedPort, nodeAdapter)) {
            compatiblePorts.TryGetValue(port.owner, out var portList);
            if (portList == null)
                portList = compatiblePorts[port.owner] = new List<PortView>();
            portList.Add(port);
        }

        // Sort ports by position in the node
        foreach (var kp in compatiblePorts)
            kp.Value.Sort((e1, e2) => e1.worldBound.y.CompareTo(e2.worldBound.y));

        // Only light compatible anchors when dragging an edge.
        graphView.ports.ForEach(p => { p.OnStartEdgeDragging(); });

        foreach (var kp in compatiblePorts)
        foreach (var port in kp.Value)
            port.highlight = true;

        edgeCandidate.UpdateEdgeControl();

        if (panSchedule == null) {
            panSchedule = graphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
            panSchedule.Pause();
        }

        wasPanned = false;

        edgeCandidate.layer = int.MaxValue;

        return true;
    }

    internal Vector2 GetEffectivePanSpeed(Vector2 mousePos) {
        var effectiveSpeed = Vector2.zero;

        if (mousePos.x <= k_PanAreaWidth)
            effectiveSpeed.x = -((k_PanAreaWidth - mousePos.x) / k_PanAreaWidth + 0.5f) * k_PanSpeed;
        else if (mousePos.x >= graphView.contentContainer.layout.width - k_PanAreaWidth)
            effectiveSpeed.x =
                ((mousePos.x - (graphView.contentContainer.layout.width - k_PanAreaWidth)) / k_PanAreaWidth + 0.5f) *
                k_PanSpeed;

        if (mousePos.y <= k_PanAreaWidth)
            effectiveSpeed.y = -((k_PanAreaWidth - mousePos.y) / k_PanAreaWidth + 0.5f) * k_PanSpeed;
        else if (mousePos.y >= graphView.contentContainer.layout.height - k_PanAreaWidth)
            effectiveSpeed.y =
                ((mousePos.y - (graphView.contentContainer.layout.height - k_PanAreaWidth)) / k_PanAreaWidth + 0.5f) *
                k_PanSpeed;

        effectiveSpeed = Vector2.ClampMagnitude(effectiveSpeed, k_MaxPanSpeed);

        return effectiveSpeed;
    }

    public override void HandleMouseMove(MouseMoveEvent evt) {
        var ve = (VisualElement)evt.target;
        var gvMousePos = ve.ChangeCoordinatesTo(graphView.contentContainer, evt.localMousePosition);
        panDiff = GetEffectivePanSpeed(gvMousePos);

        if (panDiff != Vector3.zero)
            panSchedule.Resume();
        else
            panSchedule.Pause();

        var mousePosition = evt.mousePosition;
        lastMousePos = evt.mousePosition;

        edgeCandidate.candidatePosition = mousePosition;

        // Draw ghost edge if possible port exists.
        var endPort = GetEndPort(mousePosition);

        if (endPort != null) {
            if (ghostEdge == null) {
                ghostEdge = CreateEdgeView();
                ghostEdge.isGhostEdge = true;
                ghostEdge.pickingMode = PickingMode.Ignore;
                graphView.AddElement(ghostEdge);
            }

            if (edgeCandidate.output == null) {
                ghostEdge.input = edgeCandidate.input;
                if (ghostEdge.output != null)
                    ghostEdge.output.portCapLit = false;
                ghostEdge.output = endPort;
                ghostEdge.output.portCapLit = true;
            } else {
                if (ghostEdge.input != null)
                    ghostEdge.input.portCapLit = false;
                ghostEdge.input = endPort;
                ghostEdge.input.portCapLit = true;
                ghostEdge.output = edgeCandidate.output;
            }
        } else if (ghostEdge != null) {
            if (edgeCandidate.input == null) {
                if (ghostEdge.input != null)
                    ghostEdge.input.portCapLit = false;
            } else {
                if (ghostEdge.output != null)
                    ghostEdge.output.portCapLit = false;
            }

            graphView.RemoveElement(ghostEdge);
            ghostEdge.input = null;
            ghostEdge.output = null;
            ghostEdge = null;
        }
    }

    protected virtual EdgeView CreateEdgeView() {
        return new EdgeView();
    }

    private void Pan(TimerState ts) {
        graphView.viewTransform.position -= panDiff;

        // Workaround to force edge to update when we pan the graph
        edgeCandidate.output = edgeCandidate.output;
        edgeCandidate.input = edgeCandidate.input;

        edgeCandidate.UpdateEdgeControl();
        wasPanned = true;
    }

    public override void HandleMouseUp(MouseUpEvent evt) {
        var didConnect = false;

        var mousePosition = evt.mousePosition;

        // Reset the highlights.
        graphView.ports.ForEach(p => { p.OnStopEdgeDragging(); });

        // Clean up ghost edges.
        if (ghostEdge != null) {
            if (ghostEdge.input != null)
                ghostEdge.input.portCapLit = false;
            if (ghostEdge.output != null)
                ghostEdge.output.portCapLit = false;

            graphView.RemoveElement(ghostEdge);
            ghostEdge.input = null;
            ghostEdge.output = null;
            ghostEdge = null;
        }

        var endPort = GetEndPort(mousePosition);

        if (endPort == null && listener != null) listener.OnDropOutsidePort(edgeCandidate, mousePosition);

        edgeCandidate.SetEnabled(true);

        if (edgeCandidate.input != null)
            edgeCandidate.input.portCapLit = false;

        if (edgeCandidate.output != null)
            edgeCandidate.output.portCapLit = false;

        // If it is an existing valid edge then delete and notify the model (using DeleteElements()).
        if (edgeCandidate.input != null && edgeCandidate.output != null) {
            // Save the current input and output before deleting the edge as they will be reset
            var oldInput = edgeCandidate.input;
            var oldOutput = edgeCandidate.output;

            graphView.DeleteElements(new[] { edgeCandidate });

            // Restore the previous input and output
            edgeCandidate.input = oldInput;
            edgeCandidate.output = oldOutput;
        }
        // otherwise, if it is an temporary edge then just remove it as it is not already known my the model
        else {
            graphView.RemoveElement(edgeCandidate);
        }

        if (endPort != null) {
            if (endPort.direction == Direction.Output)
                edgeCandidate.output = endPort;
            else
                edgeCandidate.input = endPort;

            listener.OnDrop(graphView, edgeCandidate);
            didConnect = true;
        } else {
            edgeCandidate.output = null;
            edgeCandidate.input = null;
        }

        edgeCandidate.ResetLayer();

        edgeCandidate = null;
        compatiblePorts.Clear();
        Reset(didConnect);
    }

    private Rect GetPortBounds(BaseNodeView nodeView, int index, List<PortView> portList) {
        var port = portList[index];
        var bounds = port.worldBound;

        if (port.orientation == Orientation.Horizontal) {
            // Increase horizontal port bounds
            bounds.xMin = nodeView.worldBound.xMin;
            bounds.xMax = nodeView.worldBound.xMax;

            if (index == 0)
                bounds.yMin = nodeView.worldBound.yMin;
            if (index == portList.Count - 1)
                bounds.yMax = nodeView.worldBound.yMax;

            if (index > 0) {
                var above = portList[index - 1].worldBound;
                bounds.yMin = (above.yMax + bounds.yMin) / 2.0f;
            }

            if (index < portList.Count - 1) {
                var below = portList[index + 1].worldBound;
                bounds.yMax = (below.yMin + bounds.yMax) / 2.0f;
            }

            if (port.direction == Direction.Input)
                bounds.xMin -= kPortDetectionWidth;
            else
                bounds.xMax += kPortDetectionWidth;
        } else {
            // Increase vertical port bounds
            if (port.direction == Direction.Input)
                bounds.yMin -= kPortDetectionWidth;
            else
                bounds.yMax += kPortDetectionWidth;
        }

        return bounds;
    }

    private Port GetEndPort(Vector2 mousePosition) {
        if (graphView == null)
            return null;

        Port bestPort = null;
        var bestDistance = 1e20f;

        foreach (var kp in compatiblePorts) {
            var nodeView = kp.Key;
            var portList = kp.Value;

            // We know that the port in the list is top to bottom in term of layout
            for (var i = 0; i < portList.Count; i++) {
                var port = portList[i];
                var bounds = GetPortBounds(nodeView, i, portList);

                var distance = Vector2.Distance(port.worldBound.position, mousePosition);

                // Check if mouse is over port.
                if (bounds.Contains(mousePosition) && distance < bestDistance) {
                    bestPort = port;
                    bestDistance = distance;
                }
            }
        }

        return bestPort;
    }
}
}