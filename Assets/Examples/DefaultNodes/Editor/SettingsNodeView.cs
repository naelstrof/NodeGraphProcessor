using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(SettingsNode))]
public class SettingsNodeView : BaseNodeView {
    private SettingsNode settingsNode;
    protected override bool hasSettings => true;

    public override void Enable() {
        settingsNode = nodeTarget as SettingsNode;

        controlsContainer.Add(new Label("Hello World !"));
    }

    protected override VisualElement CreateSettingsView() {
        var settings = new VisualElement();

        settings.Add(new EnumField("S", settingsNode.setting));

        return settings;
    }
}