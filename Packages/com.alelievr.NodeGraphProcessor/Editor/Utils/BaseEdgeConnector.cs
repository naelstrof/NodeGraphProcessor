using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class BaseEdgeConnector : EdgeConnector {
    internal const float k_ConnectionDistanceTreshold = 10f;
    protected bool active;
    protected BaseEdgeDragHelper dragHelper;
    private Edge edgeCandidate;
    protected BaseGraphView graphView;
    private Vector2 mouseDownPosition;

    public BaseEdgeConnector(IEdgeConnectorListener listener) {
        graphView = (listener as BaseEdgeConnectorListener)?.graphView;
        active = false;
        InitEdgeConnector(listener);
    }

    public override EdgeDragHelper edgeDragHelper => dragHelper;

    protected virtual void InitEdgeConnector(IEdgeConnectorListener listener) {
        dragHelper = new BaseEdgeDragHelper(listener);
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget() {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
    }

    protected override void UnregisterCallbacksFromTarget() {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
    }

    protected virtual void OnMouseDown(MouseDownEvent e) {
        if (active) {
            e.StopImmediatePropagation();
            return;
        }

        if (!CanStartManipulation(e)) return;

        var graphElement = target as Port;
        if (graphElement == null) return;

        mouseDownPosition = e.localMousePosition;

        edgeCandidate = graphView != null ? graphView.CreateEdgeView() : new EdgeView();
        edgeDragHelper.draggedPort = graphElement;
        edgeDragHelper.edgeCandidate = edgeCandidate;

        if (edgeDragHelper.HandleMouseDown(e)) {
            active = true;
            target.CaptureMouse();

            e.StopPropagation();
        } else {
            edgeDragHelper.Reset();
            edgeCandidate = null;
        }
    }

    private void OnCaptureOut(MouseCaptureOutEvent e) {
        active = false;
        if (edgeCandidate != null)
            Abort();
    }

    protected virtual void OnMouseMove(MouseMoveEvent e) {
        if (!active) return;

        edgeDragHelper.HandleMouseMove(e);
        edgeCandidate.candidatePosition = e.mousePosition;
        edgeCandidate.UpdateEdgeControl();
        e.StopPropagation();
    }

    protected virtual void OnMouseUp(MouseUpEvent e) {
        if (!active || !CanStopManipulation(e))
            return;

        if (CanPerformConnection(e.localMousePosition))
            edgeDragHelper.HandleMouseUp(e);
        else
            Abort();

        active = false;
        edgeCandidate = null;
        target.ReleaseMouse();
        e.StopPropagation();
    }

    private void OnKeyDown(KeyDownEvent e) {
        if (e.keyCode != KeyCode.Escape || !active)
            return;

        Abort();

        active = false;
        target.ReleaseMouse();
        e.StopPropagation();
    }

    private void Abort() {
        var graphView = target?.GetFirstAncestorOfType<GraphView>();
        graphView?.RemoveElement(edgeCandidate);

        edgeCandidate.input = null;
        edgeCandidate.output = null;
        edgeCandidate = null;

        edgeDragHelper.Reset();
    }

    private bool CanPerformConnection(Vector2 mousePosition) {
        return Vector2.Distance(mouseDownPosition, mousePosition) > k_ConnectionDistanceTreshold;
    }
}
}