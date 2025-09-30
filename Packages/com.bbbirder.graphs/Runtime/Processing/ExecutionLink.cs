#if UNITY_EDITOR
using UnityEditor;
#endif

using BBBirder;

namespace GraphProcessor
{
    public struct ExecutionLink
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            RuntimeConverter.RegisterDisallowedConversion<ExecutionLink, object>();
            RuntimeConverter.RegisterDisallowedConversion<object, ExecutionLink>();
            RuntimeConverter.RegisterDisallowedConversion<ExecutionLink, string>();
            RuntimeConverter.RegisterDisallowedConversion<string, ExecutionLink>();
        }
#endif
    }
}
