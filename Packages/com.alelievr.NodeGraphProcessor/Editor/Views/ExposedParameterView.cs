using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class ExposedParameterView : PinnedElementView {
    private new const string title = "Parameters";

    private readonly string exposedParameterViewStyle = "GraphProcessorStyles/ExposedParameterView";

    private List<Rect> blackboardLayouts = new();
    protected BaseGraphView graphView;

    public ExposedParameterView() {
        var style = Resources.Load<StyleSheet>(exposedParameterViewStyle);
        if (style != null)
            styleSheets.Add(style);
    }

    protected virtual void OnAddClicked() {
        var parameterType = new GenericMenu();

        foreach (var paramType in GetExposedParameterTypes())
            parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () => {
                var uniqueName = "New " + GetNiceNameFromType(paramType);

                uniqueName = GetUniqueExposedPropertyName(uniqueName);
                graphView.graph.AddExposedParameter(uniqueName, paramType);
            });

        parameterType.ShowAsContext();
    }

    protected string GetNiceNameFromType(Type type) {
        var name = type.Name;

        // Remove parameter in the name of the type if it exists
        name = name.Replace("Parameter", "");

        return ObjectNames.NicifyVariableName(name);
    }

    protected string GetUniqueExposedPropertyName(string name) {
        // Generate unique name
        var uniqueName = name;
        var i = 0;
        while (graphView.graph.exposedParameters.Any(e => e.name == name))
            name = uniqueName + " " + i++;
        return name;
    }

    protected virtual IEnumerable<Type> GetExposedParameterTypes() {
        foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>()) {
            if (type.IsGenericType)
                continue;

            yield return type;
        }
    }

    protected virtual void UpdateParameterList() {
        content.Clear();

        foreach (var param in graphView.graph.exposedParameters) {
            var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param),
                new ExposedParameterPropertyView(graphView, param));
            row.expanded = param.settings.expanded;
            row.RegisterCallback<GeometryChangedEvent>(e => { param.settings.expanded = row.expanded; });

            content.Add(row);
        }
    }

    protected override void Initialize(BaseGraphView graphView) {
        this.graphView = graphView;
        base.title = title;
        scrollable = true;

        graphView.onExposedParameterListChanged += UpdateParameterList;
        graphView.initialized += UpdateParameterList;
        Undo.undoRedoPerformed += UpdateParameterList;

        RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
        RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
        RegisterCallback<DetachFromPanelEvent>(OnViewClosed);

        UpdateParameterList();

        // Add exposed parameter button
        header.Add(new Button(OnAddClicked) {
            text = "+"
        });
    }

    private void OnViewClosed(DetachFromPanelEvent evt) {
        Undo.undoRedoPerformed -= UpdateParameterList;
    }

    private void OnMouseDownEvent(MouseDownEvent evt) {
        blackboardLayouts = content.Children().Select(c => c.layout).ToList();
    }

    private int GetInsertIndexFromMousePosition(Vector2 pos) {
        pos = content.WorldToLocal(pos);
        // We only need to look for y axis;
        var mousePos = pos.y;

        if (mousePos < 0)
            return 0;

        var index = 0;
        foreach (var layout in blackboardLayouts) {
            if (mousePos > layout.yMin && mousePos < layout.yMax)
                return index + 1;
            index++;
        }

        return content.childCount;
    }

    private void OnDragUpdatedEvent(DragUpdatedEvent evt) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        var newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
        var graphSelectionDragData = DragAndDrop.GetGenericData("DragSelection");

        if (graphSelectionDragData == null)
            return;

        foreach (var obj in graphSelectionDragData as List<ISelectable>)
            if (obj is ExposedParameterFieldView view) {
                var blackBoardRow = view.parent.parent.parent.parent.parent.parent;
                var oldIndex = content.Children().ToList().FindIndex(c => c == blackBoardRow);
                // Try to find the blackboard row
                content.Remove(blackBoardRow);

                if (newIndex > oldIndex)
                    newIndex--;

                content.Insert(newIndex, blackBoardRow);
            }
    }

    private void OnDragPerformEvent(DragPerformEvent evt) {
        var updateList = false;

        var newIndex = GetInsertIndexFromMousePosition(evt.mousePosition);
        foreach (var obj in DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>)
            if (obj is ExposedParameterFieldView view) {
                if (!updateList)
                    graphView.RegisterCompleteObjectUndo("Moved parameters");

                var oldIndex = graphView.graph.exposedParameters.FindIndex(e => e == view.parameter);
                var parameter = graphView.graph.exposedParameters[oldIndex];
                graphView.graph.exposedParameters.RemoveAt(oldIndex);

                // Patch new index after the remove operation:
                if (newIndex > oldIndex)
                    newIndex--;

                graphView.graph.exposedParameters.Insert(newIndex, parameter);

                updateList = true;
            }

        if (updateList) {
            graphView.graph.NotifyExposedParameterListChanged();
            evt.StopImmediatePropagation();
            UpdateParameterList();
        }
    }
}
}