﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
public class PortView : Port {
    private readonly List<EdgeView> edges = new();

    private readonly string portStyle = "GraphProcessorStyles/PortView";

    private readonly string userPortStyleFile = "PortViewTypes";

    protected FieldInfo fieldInfo;
    protected BaseEdgeConnectorListener listener;
    public PortData portData;
    public new Type portType;

    protected PortView(Direction direction, FieldInfo fieldInfo, PortData portData,
        BaseEdgeConnectorListener edgeConnectorListener)
        : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi,
            portData.displayType ?? fieldInfo.FieldType) {
        this.fieldInfo = fieldInfo;
        listener = edgeConnectorListener;
        portType = portData.displayType ?? fieldInfo.FieldType;
        this.portData = portData;
        portName = fieldName;

        styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

        UpdatePortSize();

        var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
        if (userPortStyle != null)
            styleSheets.Add(userPortStyle);

        if (portData.vertical)
            AddToClassList("Vertical");

        tooltip = portData.tooltip;
    }

    public string fieldName => fieldInfo.Name;
    public Type fieldType => fieldInfo.FieldType;
    public BaseNodeView owner { get; private set; }

    public int connectionCount => edges.Count;

    public event Action<PortView, Edge> OnConnected;
    public event Action<PortView, Edge> OnDisconnected;

    public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData,
        BaseEdgeConnectorListener edgeConnectorListener) {
        var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
        pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
        pv.AddManipulator(pv.m_EdgeConnector);

        // Force picking in the port label to enlarge the edge creation zone
        var portLabel = pv.Q("type");
        if (portLabel != null) {
            portLabel.pickingMode = PickingMode.Position;
            portLabel.style.flexGrow = 1;
        }

        // hide label when the port is vertical
        if (portData.vertical && portLabel != null)
            portLabel.style.display = DisplayStyle.None;

        // Fixup picking mode for vertical top ports
        if (portData.vertical)
            pv.Q("connector").pickingMode = PickingMode.Position;

        return pv;
    }

    /// <summary>
    ///     Update the size of the port view (using the portData.sizeInPixel property)
    /// </summary>
    public void UpdatePortSize() {
        var size = portData.sizeInPixel == 0 ? 8 : portData.sizeInPixel;
        var connector = this.Q("connector");
        var cap = connector.Q("cap");
        connector.style.width = size;
        connector.style.height = size;
        cap.style.width = size - 4;
        cap.style.height = size - 4;

        // Update connected edge sizes:
        edges.ForEach(e => e.UpdateEdgeSize());
    }

    public virtual void Initialize(BaseNodeView nodeView, string name) {
        owner = nodeView;
        AddToClassList(fieldName);

        // Correct port type if port accept multiple values (and so is a container)
        if (direction == Direction.Input && portData.acceptMultipleEdges &&
            portType == fieldType) // If the user haven't set a custom field type
            if (fieldType.GetGenericArguments().Length > 0)
                portType = fieldType.GetGenericArguments()[0];

        if (name != null)
            portName = name;
        visualClass = "Port_" + portType.Name;
        tooltip = portData.tooltip;
    }

    public override void Connect(Edge edge) {
        OnConnected?.Invoke(this, edge);

        base.Connect(edge);

        var inputNode = (edge.input as PortView).owner;
        var outputNode = (edge.output as PortView).owner;

        edges.Add(edge as EdgeView);

        inputNode.OnPortConnected(edge.input as PortView);
        outputNode.OnPortConnected(edge.output as PortView);
    }

    public override void Disconnect(Edge edge) {
        OnDisconnected?.Invoke(this, edge);

        base.Disconnect(edge);

        if (!(edge as EdgeView).isConnected)
            return;

        var inputNode = (edge.input as PortView)?.owner;
        var outputNode = (edge.output as PortView)?.owner;

        inputNode?.OnPortDisconnected(edge.input as PortView);
        outputNode?.OnPortDisconnected(edge.output as PortView);

        edges.Remove(edge as EdgeView);
    }

    public void UpdatePortView(PortData data) {
        if (data.displayType != null) {
            base.portType = data.displayType;
            portType = data.displayType;
            visualClass = "Port_" + portType.Name;
        }

        if (!string.IsNullOrEmpty(data.displayName))
            portName = data.displayName;

        portData = data;

        // Update the edge in case the port color have changed
        schedule.Execute(() => {
            foreach (var edge in edges) {
                edge.UpdateEdgeControl();
                edge.MarkDirtyRepaint();
            }
        }).ExecuteLater(50); // Hummm

        UpdatePortSize();
    }

    public List<EdgeView> GetEdges() {
        return edges;
    }
}
}