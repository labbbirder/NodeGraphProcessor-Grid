using System;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
    public class GraphBehaviour<T> : MonoBehaviour, IGraphOwner<T> where T : BaseGraph
    {
        public bool useTemplate;
        [SerializeField] private GraphTemplate<T> template;
        [SerializeField] private T graph;
        [NonSerialized] private T resolvedGraph;

        public T Graph
        {
            get
            {
                if (resolvedGraph != null) return resolvedGraph;

                if (useTemplate)
                {
                    var copy = Instantiate(template);
                    copy.hideFlags = HideFlags.HideAndDontSave;
                    resolvedGraph = copy.Graph;
                }
                else
                {
                    resolvedGraph = graph;
                }

                resolvedGraph.unityObject = this;
                return resolvedGraph;
            }
        }

        protected virtual void OnEnable()
        {
            Graph.Initialize();
        }

        protected virtual void OnDisable()
        {
            Graph.Deinitialize();
        }

#if UNITY_EDITOR
        public void CreateSerialized(out SerializedObject serializedObject, out SerializedProperty graphProperty)
        {
            if (useTemplate)
            {
                template.CreateSerialized(out serializedObject, out graphProperty);
            }
            else
            {
                serializedObject = new(this);
                graphProperty = serializedObject.FindProperty(nameof(graph));
            }
        }
#endif
    }
}