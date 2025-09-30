using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
    public static class ResUtils
    {
        public static T Load<T>(string relativePath, [CallerFilePath] string callerPath = null) where T : UnityEngine.Object
        {
            var result = AssetDatabase.LoadAssetAtPath<T>(GetFullPath(relativePath, callerPath));
            if (!result)
            {
                Debug.LogWarning($"Cannot load asset at {relativePath} with type {typeof(T)}");
            }
            return result;
        }

        static string GetFullPath(string relativePath, [CallerFilePath] string callerPath = null)
        {
            return Path.GetRelativePath(".", Path.Combine(callerPath, "..", relativePath)).Replace("\\", "/");
        }
    }
}
