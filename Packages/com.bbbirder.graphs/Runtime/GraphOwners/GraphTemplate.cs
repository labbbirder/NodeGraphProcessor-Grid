#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace GraphProcessor
{
    public interface IGraphTemplate
    {
        void SetGraph(BaseGraph graph);
    }

    public class GraphTemplate<T> : ScriptableObject, IGraphOwner<T>, IGraphTemplate where T : BaseGraph
    {
        [SerializeField] private T graph;

        public T Graph
        {
            get
            {
                graph.unityObject = this;
                return graph;
            }
        }

        protected virtual void OnEnable()
        {
            graph?.Initialize();
        }

        protected virtual void OnDisable()
        {
            graph?.Deinitialize();
        }

#if UNITY_EDITOR
        public void CreateSerialized(out SerializedObject serializedObject, out SerializedProperty graphProperty)
        {
            serializedObject = new(this);
            graphProperty = serializedObject.FindProperty(nameof(graph));
        }

#endif

        public void SetGraph(BaseGraph graph)
        {
            this.graph = graph as T;
            graph?.Initialize();
        }
    }
}
