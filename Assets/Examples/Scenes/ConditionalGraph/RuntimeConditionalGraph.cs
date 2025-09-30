using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine;

public class RuntimeConditionalGraph : MonoBehaviour
{
	[Header("Graph to Run on Start")]
	public BaseGraph graph;

	// private ExecutionProcessor processor;

	// private void Start()
	// {
	// 	if (graph != null)
	// 		processor = new ExecutionProcessor(graph);

	// 	processor.Run();
	// }
}
