using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor {
public class ToolbarView : VisualElement {
    private readonly List<ToolbarButtonData> leftButtonDatas = new();
    private readonly List<ToolbarButtonData> rightButtonDatas = new();
    protected BaseGraphView graphView;
    private ToolbarButtonData showParameters;

    private ToolbarButtonData showProcessor;

    public ToolbarView(BaseGraphView graphView) {
        name = "ToolbarView";
        this.graphView = graphView;

        graphView.initialized += () => {
            leftButtonDatas.Clear();
            rightButtonDatas.Clear();
            AddButtons();
        };

        Add(new IMGUIContainer(DrawImGUIToolbar));
    }

    protected ToolbarButtonData AddButton(string name, Action callback, bool left = true) {
        return AddButton(new GUIContent(name), callback, left);
    }

    protected ToolbarButtonData AddButton(GUIContent content, Action callback, bool left = true) {
        var data = new ToolbarButtonData {
            content = content,
            type = ElementType.Button,
            buttonCallback = callback
        };
        (left ? leftButtonDatas : rightButtonDatas).Add(data);
        return data;
    }

    protected void AddSeparator(int sizeInPixels = 10, bool left = true) {
        var data = new ToolbarButtonData {
            type = ElementType.Separator,
            size = sizeInPixels
        };
        (left ? leftButtonDatas : rightButtonDatas).Add(data);
    }

    protected void AddCustom(Action imguiDrawFunction, bool left = true) {
        if (imguiDrawFunction == null)
            throw new ArgumentException("imguiDrawFunction can't be null");

        var data = new ToolbarButtonData {
            type = ElementType.Custom,
            customDrawFunction = imguiDrawFunction
        };
        (left ? leftButtonDatas : rightButtonDatas).Add(data);
    }

    protected void AddFlexibleSpace(bool left = true) {
        (left ? leftButtonDatas : rightButtonDatas).Add(new ToolbarButtonData { type = ElementType.FlexibleSpace });
    }

    protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action<bool> callback, bool left = true) {
        return AddToggle(new GUIContent(name), defaultValue, callback, left);
    }

    protected ToolbarButtonData AddToggle(GUIContent content, bool defaultValue, Action<bool> callback,
        bool left = true) {
        var data = new ToolbarButtonData {
            content = content,
            type = ElementType.Toggle,
            value = defaultValue,
            toggleCallback = callback
        };
        (left ? leftButtonDatas : rightButtonDatas).Add(data);
        return data;
    }

    protected ToolbarButtonData AddDropDownButton(string name, Action callback, bool left = true) {
        return AddDropDownButton(new GUIContent(name), callback, left);
    }

    protected ToolbarButtonData AddDropDownButton(GUIContent content, Action callback, bool left = true) {
        var data = new ToolbarButtonData {
            content = content,
            type = ElementType.DropDownButton,
            buttonCallback = callback
        };
        (left ? leftButtonDatas : rightButtonDatas).Add(data);
        return data;
    }

    /// <summary>
    ///     Also works for toggles
    /// </summary>
    /// <param name="name"></param>
    /// <param name="left"></param>
    protected void RemoveButton(string name, bool left) {
        (left ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
    }

    /// <summary>
    ///     Hide the button
    /// </summary>
    /// <param name="name">Display name of the button</param>
    protected void HideButton(string name) {
        leftButtonDatas.Concat(rightButtonDatas).All(b => {
            if (b?.content?.text == name)
                b.visible = false;
            return true;
        });
    }

    /// <summary>
    ///     Show the button
    /// </summary>
    /// <param name="name">Display name of the button</param>
    protected void ShowButton(string name) {
        leftButtonDatas.Concat(rightButtonDatas).All(b => {
            if (b?.content?.text == name)
                b.visible = true;
            return true;
        });
    }

    protected virtual void AddButtons() {
        AddButton("Center", graphView.ResetPositionAndZoom);

        var processorVisible = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
        showProcessor = AddToggle("Show Processor", processorVisible, v => graphView.ToggleView<ProcessorView>());
        var exposedParamsVisible = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
        showParameters = AddToggle("Show Parameters", exposedParamsVisible,
            v => graphView.ToggleView<ExposedParameterView>());

        AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
    }

    public virtual void UpdateButtonStatus() {
        if (showProcessor != null)
            showProcessor.value = graphView.GetPinnedElementStatus<ProcessorView>() != Status.Hidden;
        if (showParameters != null)
            showParameters.value = graphView.GetPinnedElementStatus<ExposedParameterView>() != Status.Hidden;
    }

    private void DrawImGUIButtonList(List<ToolbarButtonData> buttons) {
        foreach (var button in buttons.ToList()) {
            if (!button.visible)
                continue;

            switch (button.type) {
                case ElementType.Button:
                    if (GUILayout.Button(button.content, EditorStyles.toolbarButton) && button.buttonCallback != null)
                        button.buttonCallback();
                    break;
                case ElementType.Toggle:
                    EditorGUI.BeginChangeCheck();
                    button.value = GUILayout.Toggle(button.value, button.content, EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck() && button.toggleCallback != null)
                        button.toggleCallback(button.value);
                    break;
                case ElementType.DropDownButton:
                    if (EditorGUILayout.DropdownButton(button.content, FocusType.Passive, EditorStyles.toolbarDropDown))
                        button.buttonCallback();
                    break;
                case ElementType.Separator:
                    EditorGUILayout.Separator();
                    EditorGUILayout.Space(button.size);
                    break;
                case ElementType.Custom:
                    button.customDrawFunction();
                    break;
                case ElementType.FlexibleSpace:
                    GUILayout.FlexibleSpace();
                    break;
            }
        }
    }

    protected virtual void DrawImGUIToolbar() {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        DrawImGUIButtonList(leftButtonDatas);

        GUILayout.FlexibleSpace();

        DrawImGUIButtonList(rightButtonDatas);

        GUILayout.EndHorizontal();
    }

    protected enum ElementType {
        Button,
        Toggle,
        DropDownButton,
        Separator,
        Custom,
        FlexibleSpace
    }

    protected class ToolbarButtonData {
        public Action buttonCallback;
        public GUIContent content;
        public Action customDrawFunction;
        public int size;
        public Action<bool> toggleCallback;
        public ElementType type;
        public bool value;
        public bool visible = true;
    }
}
}