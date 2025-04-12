using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(ParameterNode))]
public class ParameterNodeView : BaseNodeView {
    private ParameterNode parameterNode;

    public override void Enable(bool fromInspector = false) {
        parameterNode = nodeTarget as ParameterNode;

        var accessorSelector = new EnumField(parameterNode.accessor);
        accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
        accessorSelector.RegisterValueChangedCallback(evt => {
            parameterNode.accessor = (ParameterAccessor)evt.newValue;
            UpdatePort();
            controlsContainer.MarkDirtyRepaint();
            ForceUpdatePorts();
        });

        UpdatePort();
        controlsContainer.Add(accessorSelector);

        //    Find and remove expand/collapse button
        titleContainer.Remove(titleContainer.Q("title-button-container"));
        //    Remove Port from the #content
        topContainer.parent.Remove(topContainer);
        //    Add Port to the #title
        titleContainer.Add(topContainer);

        parameterNode.onParameterChanged += UpdateView;
        UpdateView();
    }

    private void UpdateView() {
        title = parameterNode.parameter?.name;
    }

    private void UpdatePort() {
        if (parameterNode.accessor == ParameterAccessor.Set)
            titleContainer.AddToClassList("input");
        else
            titleContainer.RemoveFromClassList("input");
    }
}