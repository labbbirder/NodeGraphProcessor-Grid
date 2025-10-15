using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace BBBirder
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


        static T CloneObjectHelper<T>(T obj)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
        }

        static MethodInfo s_miCloneObjectHelper;
        public static object CloneObject(object obj)
        {
            var type = obj.GetType();
            s_miCloneObjectHelper ??= typeof(ResUtils).GetMethod(nameof(CloneObjectHelper), BindingFlags.Static | BindingFlags.NonPublic);
            return s_miCloneObjectHelper.MakeGenericMethod(type).Invoke(null, new[] { obj });
        }
    }
}
