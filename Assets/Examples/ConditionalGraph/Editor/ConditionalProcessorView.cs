using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine.UIElements;

public class ConditionalProcessorView : PinnedElementView {
    private BaseGraphView graphView;
    private ConditionalProcessor processor;

    public ConditionalProcessorView() {
        title = "Conditional Processor";
    }

    protected override void Initialize(BaseGraphView graphView) {
        processor = new ConditionalProcessor(graphView.graph);
        this.graphView = graphView;

        graphView.computeOrderUpdated += processor.UpdateComputeOrder;

        var runButton = new Button(OnPlay) { name = "ActionButton", text = "Run" };
        var stepButton = new Button(OnStep) { name = "ActionButton", text = "Step" };

        content.Add(runButton);
        content.Add(stepButton);
    }

    private void OnPlay() {
        processor.Run();
    }

    private void OnStep() {
        BaseNodeView view;

        if (processor.currentGraphExecution != null) {
            // Unhighlight the last executed node
            view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
            view.UnHighlight();
        }

        processor.Step();

        // Display debug infos, currentGraphExecution is modified in the Step() function above
        if (processor.currentGraphExecution != null) {
            view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
            view.Highlight();
        }
    }
}