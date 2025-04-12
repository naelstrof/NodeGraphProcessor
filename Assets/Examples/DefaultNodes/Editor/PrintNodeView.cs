using GraphProcessor;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(PrintNode))]
public class PrintNodeView : BaseNodeView {
    private Label printLabel;
    private PrintNode printNode;

    public override void Enable() {
        printNode = nodeTarget as PrintNode;

        printLabel = new Label();
        controlsContainer.Add(printLabel);

        nodeTarget.onProcessed += UpdatePrintLabel;
        onPortConnected += p => UpdatePrintLabel();
        onPortDisconnected += p => UpdatePrintLabel();

        UpdatePrintLabel();
    }

    private void UpdatePrintLabel() {
        if (printNode.obj != null)
            printLabel.text = printNode.obj.ToString();
        else
            printLabel.text = "null";
    }
}

[NodeCustomEditor(typeof(ConditionalPrintNode))]
public class ConditionalPrintNodeView : BaseNodeView {
    private Label printLabel;
    private ConditionalPrintNode printNode;

    public override void Enable() {
        printNode = nodeTarget as ConditionalPrintNode;

        printLabel = new Label();
        controlsContainer.Add(printLabel);

        nodeTarget.onProcessed += UpdatePrintLabel;
        onPortConnected += p => UpdatePrintLabel();
        onPortDisconnected += p => UpdatePrintLabel();

        UpdatePrintLabel();
    }

    private void UpdatePrintLabel() {
        if (printNode.obj != null)
            printLabel.text = printNode.obj.ToString();
        else
            printLabel.text = "null";
    }
}