

// using Unity.Entities;

namespace GraphProcessor {
/// <summary>
///     Graph processor
/// </summary>
public abstract class BaseGraphProcessor {
    protected BaseGraph graph;

    /// <summary>
    ///     Manage graph scheduling and processing
    /// </summary>
    /// <param name="graph">Graph to be processed</param>
    public BaseGraphProcessor(BaseGraph graph) {
        this.graph = graph;

        UpdateComputeOrder();
    }

    public abstract void UpdateComputeOrder();

    /// <summary>
    ///     Schedule the graph into the job system
    /// </summary>
    public abstract void Run();
}
}