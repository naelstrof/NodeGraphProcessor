using UnityEngine.UIElements;

namespace GraphProcessor {
public class ProcessorView : PinnedElementView {
    private BaseGraphProcessor processor;

    public ProcessorView() {
        title = "Process panel";
    }

    protected override void Initialize(BaseGraphView graphView) {
        processor = new ProcessGraphProcessor(graphView.graph);

        graphView.computeOrderUpdated += processor.UpdateComputeOrder;

        var b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };

        content.Add(b);
    }

    private void OnPlay() {
        processor.Run();
    }
}
}