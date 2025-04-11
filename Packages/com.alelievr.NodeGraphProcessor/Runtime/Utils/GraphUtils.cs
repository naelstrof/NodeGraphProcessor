using System;
using System.Linq;
using System.Collections.Generic;

namespace GraphProcessor {
public static class GraphUtils {
    private enum State {
        White,
        Grey,
        Black
    }

    private class TarversalNode {
        public BaseNode node;
        public List<TarversalNode> inputs = new();
        public List<TarversalNode> outputs = new();
        public State state = State.White;

        public TarversalNode(BaseNode node) {
            this.node = node;
        }
    }

    // A structure made for easy graph traversal
    private class TraversalGraph {
        public List<TarversalNode> nodes = new();
        public List<TarversalNode> outputs = new();
    }

    private static TraversalGraph ConvertGraphToTraversalGraph(BaseGraph graph) {
        var g = new TraversalGraph();
        var nodeMap = new Dictionary<BaseNode, TarversalNode>();

        foreach (var node in graph.nodes) {
            var tn = new TarversalNode(node);
            g.nodes.Add(tn);
            nodeMap[node] = tn;

            if (graph.graphOutputs.Contains(node))
                g.outputs.Add(tn);
        }

        foreach (var tn in g.nodes) {
            tn.inputs = tn.node.GetInputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();
            tn.outputs = tn.node.GetOutputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();
        }

        return g;
    }

    public static List<BaseNode> DepthFirstSort(BaseGraph g) {
        var graph = ConvertGraphToTraversalGraph(g);
        var depthFirstNodes = new List<BaseNode>();

        foreach (var n in graph.nodes)
            DFS(n);

        void DFS(TarversalNode n) {
            if (n.state == State.Black)
                return;

            n.state = State.Grey;

            if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get) {
                foreach (var setter in graph.nodes.Where(x =>
                             x.node is ParameterNode p &&
                             p.parameterGUID == parameterNode.parameterGUID &&
                             p.accessor == ParameterAccessor.Set))
                    if (setter.state == State.White)
                        DFS(setter);
            } else {
                foreach (var input in n.inputs)
                    if (input.state == State.White)
                        DFS(input);
            }

            n.state = State.Black;

            // Only add the node when his children are completely visited
            depthFirstNodes.Add(n.node);
        }

        return depthFirstNodes;
    }

    public static void FindCyclesInGraph(BaseGraph g, Action<BaseNode> cyclicNode) {
        var graph = ConvertGraphToTraversalGraph(g);
        var cyclicNodes = new List<TarversalNode>();

        foreach (var n in graph.nodes)
            DFS(n);

        void DFS(TarversalNode n) {
            if (n.state == State.Black)
                return;

            n.state = State.Grey;

            foreach (var input in n.inputs)
                if (input.state == State.White)
                    DFS(input);
                else if (input.state == State.Grey)
                    cyclicNodes.Add(n);
            n.state = State.Black;
        }

        cyclicNodes.ForEach((tn) => cyclicNode?.Invoke(tn.node));
    }
}
}