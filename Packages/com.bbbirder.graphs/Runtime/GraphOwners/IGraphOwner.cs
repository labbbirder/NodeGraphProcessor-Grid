#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GraphProcessor
{
    public interface IGraphOwner
    {
        BaseGraph Graph { get; }
        void CreateSerialized(out SerializedObject serializedObject, out SerializedProperty graphProperty);
    }

    public interface IGraphOwner<T> : IGraphOwner where T : BaseGraph
    {
        BaseGraph IGraphOwner.Graph => Graph;

        new T Graph { get; }
    }
}
