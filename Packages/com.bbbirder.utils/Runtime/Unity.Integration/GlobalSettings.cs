#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace BBBirder
{
    interface IGlobalSettings { }

    /// <summary>
    /// 全局配置类。提供Instance静态属性，并且，自动在工程目录下生成一个资源文件(默认位于Assets/Resources，可自定义路径)。<br/>
    /// Base type for global setting asset. Providing `Instance` property, 
    /// meanwhile, generating a corresponding asset file, located at Assets/Resources by default, in Project;
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public class Foo: GlobalSettings<Foo>
    /// {
    ///     static Foo()
    ///     {
    ///         /* Will be created at Assets/Resources/Foo.asset by default without the following overriding*/
    ///         // SavePath = "Assets/my-foo-settings.asset"; // override default save path
    ///         // SavePath = "ProjectSettings/my-foo-settings.asset"; // save as project setting
    ///         // SavePath = "Temp/my-foo-settings.asset"; // save as session-temporary setting
    ///         // SavePath = "Library/my-foo-settings.asset"; // save as long-term-temporary setting
    ///     } 
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public abstract class GlobalSettings<T> : ScriptableObject, IGlobalSettings where T : GlobalSettings<T>
    {
        protected static string SavePath;

        static GlobalSettings()
        {
            SavePath ??= "Assets/Resources/" + typeof(T).Name + ".asset";
        }

        private static T _instance;
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorUtility.IsPersistent(_instance))
                {
                    _instance = null;
                }

                if (!_instance)
                {
                    var savedObject = InternalEditorUtility.LoadSerializedFileAndForget(SavePath).FirstOrDefault();
                    _instance = savedObject as T ?? savedObject switch
                    {
                        GameObject go => go.GetComponent<T>(),
                        _ => null,
                    };
                }

                if (!_instance)
                {
                    var filePath = SavePath;

                    if (File.Exists(filePath))
                    {
                        Debug.LogError("Reject to create new instance for there is already one on disk:" + filePath);
                        return null;
                    }

                    string directoryName = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    _instance = CreateInstance<T>();
                    _instance.name = typeof(T).Name;

                    InternalEditorUtility.SaveToSerializedFileAndForget(new[] { _instance }, filePath, true);
                    AssetDatabase.ImportAsset(filePath);
                }
#else
                if (!_instance)
                {
                    _instance = Resources.Load<T>(typeof(T).Name);
                }

                if (!_instance)
                {
                    Debug.LogError($"No config manager found. Please ensure there is one under Resources folder with name '{typeof(T).Name}'.");
                }
#endif

                return _instance;
            }
        }

        public virtual void SaveToDisk()
        {
#if UNITY_EDITOR
            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, SavePath, true);
#endif
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Ensure an instance on proper timing.<br/>
    /// <see cref="https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html"/>
    /// </summary>
    class GlobalSettingsAssetProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (didDomainReload)
            {
                foreach (var subtype in TypeCache.GetTypesDerivedFrom<IGlobalSettings>())
                {
                    if (subtype.IsAbstract || subtype.IsInterface) continue;

                    // RuntimeHelpers.RunClassConstructor(subtype.TypeHandle); // T .cctor() may be invoked prior to GlobalSettings<T> .cctor(), so this is not reliable.
                    subtype.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)?.GetValue(null);
                }
            }
        }
    }
#endif

}
