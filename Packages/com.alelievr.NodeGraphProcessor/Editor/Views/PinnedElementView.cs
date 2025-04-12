using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public abstract class PinnedElementView : GraphElement {
    private static readonly string pinnedElementStyle = "GraphProcessorStyles/PinnedElementView";
    private static readonly string pinnedElementTree = "GraphProcessorElements/PinnedElement";

    private readonly VisualElement main;
    private readonly ScrollView scrollView;
    private readonly Label titleLabel;
    private bool _scrollable;
    protected VisualElement content;
    protected VisualElement header;
    protected PinnedElement pinnedElement;
    protected VisualElement root;

    public PinnedElementView() {
        var tpl = Resources.Load<VisualTreeAsset>(pinnedElementTree);
        styleSheets.Add(Resources.Load<StyleSheet>(pinnedElementStyle));

        main = tpl.CloneTree();
        main.AddToClassList("mainContainer");
        scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

        root = main.Q("content");

        header = main.Q("header");

        titleLabel = main.Q<Label>("titleLabel");
        content = main.Q<VisualElement>("contentContainer");

        hierarchy.Add(main);

        capabilities |= Capabilities.Movable | Capabilities.Resizable;
        style.overflow = Overflow.Hidden;

        ClearClassList();
        AddToClassList("pinnedElement");

        this.AddManipulator(new Dragger { clampToParentEdges = true });

        scrollable = false;

        hierarchy.Add(new Resizer(() => onResized?.Invoke()));

        RegisterCallback<DragUpdatedEvent>(e => { e.StopPropagation(); });

        title = "PinnedElementView";
    }

    public override string title {
        get => titleLabel.text;
        set => titleLabel.text = value;
    }

    protected bool scrollable {
        get => _scrollable;
        set {
            if (_scrollable == value)
                return;

            _scrollable = value;

            style.position = Position.Absolute;
            if (_scrollable) {
                content.RemoveFromHierarchy();
                root.Add(scrollView);
                scrollView.Add(content);
                AddToClassList("scrollable");
            } else {
                scrollView.RemoveFromHierarchy();
                content.RemoveFromHierarchy();
                root.Add(content);
                RemoveFromClassList("scrollable");
            }
        }
    }

    protected event Action onResized;

    public void InitializeGraphView(PinnedElement pinnedElement, BaseGraphView graphView) {
        this.pinnedElement = pinnedElement;
        SetPosition(pinnedElement.position);

        onResized += () => { pinnedElement.position.size = layout.size; };

        RegisterCallback<MouseUpEvent>(e => { pinnedElement.position.position = layout.position; });

        Initialize(graphView);
    }

    public void ResetPosition() {
        pinnedElement.position = new Rect(Vector2.zero, PinnedElement.defaultSize);
        SetPosition(pinnedElement.position);
    }

    protected abstract void Initialize(BaseGraphView graphView);

    ~PinnedElementView() {
        Destroy();
    }

    protected virtual void Destroy() {
    }
}
}