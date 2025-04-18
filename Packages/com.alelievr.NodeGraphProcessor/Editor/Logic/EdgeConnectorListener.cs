﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphProcessor {
/// <summary>
///     Base class to write your own edge handling connection system
/// </summary>
public class BaseEdgeConnectorListener : IEdgeConnectorListener {
    private static CreateNodeMenuWindow edgeNodeCreateMenuWindow;

    private readonly Dictionary<Edge, PortView> edgeInputPorts = new();
    private readonly Dictionary<Edge, PortView> edgeOutputPorts = new();
    public readonly BaseGraphView graphView;

    public BaseEdgeConnectorListener(BaseGraphView graphView) {
        this.graphView = graphView;
    }

    public virtual void OnDropOutsidePort(Edge edge, Vector2 position) {
        graphView.RegisterCompleteObjectUndo("Disconnect edge");

        //If the edge was already existing, remove it
        if (!edge.isGhostEdge)
            graphView.Disconnect(edge as EdgeView);

        // when on of the port is null, then the edge was created and dropped outside of a port
        if (edge.input == null || edge.output == null)
            ShowNodeCreationMenuFromEdge(edge as EdgeView, position);
    }

    public virtual void OnDrop(GraphView graphView, Edge edge) {
        var edgeView = edge as EdgeView;
        var wasOnTheSamePort = false;

        if (edgeView?.input == null || edgeView?.output == null)
            return;

        //If the edge was moved to another port
        if (edgeView.isConnected) {
            if (edgeInputPorts.ContainsKey(edge) && edgeOutputPorts.ContainsKey(edge))
                if (edgeInputPorts[edge] == edge.input && edgeOutputPorts[edge] == edge.output)
                    wasOnTheSamePort = true;

            if (!wasOnTheSamePort)
                this.graphView.Disconnect(edgeView);
        }

        if (edgeView.input.node == null || edgeView.output.node == null)
            return;

        edgeInputPorts[edge] = edge.input as PortView;
        edgeOutputPorts[edge] = edge.output as PortView;
        try {
            this.graphView.RegisterCompleteObjectUndo("Connected " + edgeView.input.node.name + " and " +
                                                      edgeView.output.node.name);
            if (!this.graphView.Connect(edge as EdgeView, !wasOnTheSamePort))
                this.graphView.Disconnect(edge as EdgeView);
        } catch (Exception) {
            this.graphView.Disconnect(edge as EdgeView);
        }
    }

    private void ShowNodeCreationMenuFromEdge(EdgeView edgeView, Vector2 position) {
        if (edgeNodeCreateMenuWindow == null)
            edgeNodeCreateMenuWindow = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();

        edgeNodeCreateMenuWindow.Initialize(graphView, EditorWindow.focusedWindow, edgeView);
        SearchWindow.Open(new SearchWindowContext(position + EditorWindow.focusedWindow.position.position),
            edgeNodeCreateMenuWindow);
    }
}
}